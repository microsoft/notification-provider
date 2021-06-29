// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Newtonsoft.Json.Linq;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;

    /// <summary>
    /// Helper class to handle token related activities.
    /// </summary>
    public class TokenHelper : ITokenHelper
    {
        /// <summary>
        /// User Token Configuration.
        /// </summary>
        private readonly UserTokenSetting userTokenSetting;

        /// <summary>
        /// MS Graph configuration.
        /// </summary>
        private readonly MSGraphSetting mSGraphSetting;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        private readonly IEmailAccountManager emailAccountManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenHelper"/> class.
        /// </summary>
        /// <param name="userTokenSetting">User token setting from configuration.</param>
        /// <param name="mSGraphSetting">MS Graph Settings from configuration.</param>
        /// <param name="logger">Instance of <see cref="ILogger"/>.</param>
        /// <param name="emailAccountManager">Instance of <see cref="IEmailAccountManager"/>.</param>
        public TokenHelper(IOptions<UserTokenSetting> userTokenSetting, IOptions<MSGraphSetting> mSGraphSetting, ILogger logger, IEmailAccountManager emailAccountManager)
        {
            if (userTokenSetting is null)
            {
                throw new System.ArgumentNullException(nameof(userTokenSetting));
            }

            if (mSGraphSetting is null)
            {
                throw new System.ArgumentNullException(nameof(mSGraphSetting));
            }

            this.userTokenSetting = userTokenSetting.Value;
            this.mSGraphSetting = mSGraphSetting.Value;
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.emailAccountManager = emailAccountManager;
        }

        /// <inheritdoc/>
        public async Task<AuthenticationHeaderValue> GetAuthenticationHeader()
        {
            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                         .Create(this.mSGraphSetting.ClientId)
                         .WithTenantId(this.mSGraphSetting.TenantId)
                         .WithClientSecret(this.mSGraphSetting.ClientCredential)
                         .Build();
            var scopes = new string[] { this.mSGraphSetting.GraphResourceId + "/.default" };
            var result = await confidentialClientApplication.AcquireTokenForClient(scopes).ExecuteAsync().ConfigureAwait(false);
            var authHeader = new AuthenticationHeaderValue(ApplicationConstants.BearerAuthenticationScheme, result.AccessToken);
            this.logger.TraceInformation($"Finished {nameof(this.GetAuthenticationHeader)} method of {nameof(TokenHelper)}.");
            return authHeader;
        }
    }
}
