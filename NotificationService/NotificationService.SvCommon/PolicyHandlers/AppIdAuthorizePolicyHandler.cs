// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.SvCommon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Contracts;

    /// <summary>
    /// Handler for evaluation of AppAuthorizeRequirement Policy.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AppIdAuthorizePolicyHandler : AuthorizationHandler<AppIdAuthorizeRequirement>
    {
        /// <summary>
        /// Instance of <see cref="IHttpContextAccessor"/>.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Instance of <see cref="IConfiguration"/>.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppIdAuthorizePolicyHandler"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Instance of <see cref="IHttpContextAccessor"/>.</param>
        /// <param name="configuration">Instance of <see cref="IConfiguration"/>.</param>
        public AppIdAuthorizePolicyHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc/>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AppIdAuthorizeRequirement requirement)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (requirement is null)
            {
                throw new ArgumentNullException(nameof(requirement));
            }

            var httpContext = this.httpContextAccessor.HttpContext;

            var headers = httpContext?.Request?.Headers;
            var routeValues = httpContext?.Request?.RouteValues;

            if (headers != null && headers.ContainsKey(ApplicationConstants.AuthorizationHeaderName) && headers[ApplicationConstants.AuthorizationHeaderName].Count >= 1 &&
                headers[ApplicationConstants.AuthorizationHeaderName][0].StartsWith(ApplicationConstants.BearerAuthenticationScheme, StringComparison.OrdinalIgnoreCase))
            {
                var token = headers[ApplicationConstants.AuthorizationHeaderName][0].Substring($"{ApplicationConstants.BearerAuthenticationScheme} ".Length);
                var tokenHandler = new JwtSecurityTokenHandler();
                var claims = tokenHandler.ReadJwtToken(token).Claims;
                var currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var expiryInToken = int.Parse(claims.FirstOrDefault(c => c.Type == ApplicationConstants.ExpiryClaimType).Value, CultureInfo.InvariantCulture);

                // Validate further only if the token is still valid.
                if (expiryInToken > currentUnixTime)
                {
                    var appIdInToken = claims.FirstOrDefault(c => c.Type == ApplicationConstants.AppIdClaimType || c.Type == ApplicationConstants.AppIdV2ClaimType).Value;
                    var applicationName = routeValues.FirstOrDefault(rv => string.Equals(rv.Key, ApplicationConstants.ApplicationNameQueryParameter, System.StringComparison.InvariantCultureIgnoreCase)).Value?.ToString();
                    var applicationAccounts = JsonConvert.DeserializeObject<List<ApplicationAccounts>>(this.configuration[ConfigConstants.ApplicationAccountsConfigSectionKey]);
                    var validAppIdsForApplication = applicationAccounts?.Find(a => string.Equals(a.ApplicationName, applicationName, StringComparison.InvariantCultureIgnoreCase))?.ValidAppIdsList;

                    if (appIdInToken != null && validAppIdsForApplication != null && validAppIdsForApplication.Contains(appIdInToken))
                    {
                        context.Succeed(requirement);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
