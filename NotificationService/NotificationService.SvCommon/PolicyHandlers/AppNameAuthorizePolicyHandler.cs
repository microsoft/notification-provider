// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.SvCommon
{
    using System.Collections.Generic;
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
    /// Handler for evaluation of AppNameAuthorizeRequirement Policy.
    /// </summary>
    public class AppNameAuthorizePolicyHandler : AuthorizationHandler<AppNameAuthorizeRequirement>
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
        /// Initializes a new instance of the <see cref="AppNameAuthorizePolicyHandler"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Instance of <see cref="IHttpContextAccessor"/>.</param>
        /// <param name="configuration">Instance of <see cref="IConfiguration"/>.</param>
        public AppNameAuthorizePolicyHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new System.ArgumentNullException(nameof(httpContextAccessor));
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc/>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AppNameAuthorizeRequirement requirement)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            if (requirement is null)
            {
                throw new System.ArgumentNullException(nameof(requirement));
            }

            var httpContext = this.httpContextAccessor?.HttpContext;
            var routeValues = httpContext?.Request?.RouteValues;
            var applicationNameQueryParam = routeValues?.FirstOrDefault(rv => string.Equals(rv.Key, ApplicationConstants.ApplicationNameQueryParameter, System.StringComparison.InvariantCultureIgnoreCase)).Value?.ToString();
            var applicationAccountsConfiguration = JsonConvert.DeserializeObject<List<ApplicationAccounts>>(this.configuration[ConfigConstants.ApplicationAccountsConfigSectionKey]);
            if (applicationAccountsConfiguration != null && applicationAccountsConfiguration.Exists(a => a.ApplicationName == applicationNameQueryParam))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
