// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Azure.Core;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using Newtonsoft.Json.Linq;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using Azure.Core;
    using Azure.Identity;
    using Microsoft.Identity.Web;
    using static System.Formats.Asn1.AsnWriter;
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

        /// <summary>
        /// return the authentication header for selected account.
        /// </summary>
        /// <param name="selectedAccountCredential">selectedAccountCredential.</param>
        /// <returns>AuthenticationHeaderValue.</returns>
        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderValueForSelectedAccount(AccountCredential selectedAccountCredential)
        {
            AuthenticationHeaderValue authenticationHeaderValue = await this.GetAuthenticationHeaderFromToken(await this.GetAccessTokenForSelectedAccount(selectedAccountCredential).ConfigureAwait(false)).ConfigureAwait(false);
            return authenticationHeaderValue;
        }

        /// <inheritdoc/>
        public async Task<string> GetAccessTokenForSelectedAccount(AccountCredential selectedAccountCredential)
        {
            var traceProps = new Dictionary<string, string>();
            if (selectedAccountCredential == null)
            {
                throw new ArgumentNullException(nameof(selectedAccountCredential));
            }

            traceProps[AIConstants.EmailAccount] = selectedAccountCredential.AccountName;

            this.logger.TraceInformation($"Started {nameof(this.GetAccessTokenForSelectedAccount)} method of {nameof(TokenHelper)}.", traceProps);
            string authority = this.userTokenSetting.Authority;
            string clientId = this.userTokenSetting.ClientId;
            string userEmail = selectedAccountCredential?.AccountName;
            string userPassword = System.Web.HttpUtility.UrlEncode(selectedAccountCredential.PrimaryPassword);
            var token = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                var tokenEndpoint = $"{authority}";
                var accept = ApplicationConstants.JsonMIMEType;
                client.DefaultRequestHeaders.Add("Accept", accept);
                string postBody = $"resource={clientId}&client_id={clientId}&grant_type=password&username={userEmail}&password={userPassword}&scope=openid";

                using (var response = await client.PostAsync(tokenEndpoint, new StringContent(postBody, Encoding.UTF8, "application/x-www-form-urlencoded")).ConfigureAwait(false)) // CodeQL [SM00417] This is not set to the user, this is AAD call get token for a service account
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonresult = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                        token = (string)jsonresult["access_token"];
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        traceProps["Message"] = errorResponse;
                        this.logger.TraceInformation($"An error occurred while fetching token for {selectedAccountCredential.AccountName}. Details: {errorResponse}", traceProps);
                        this.logger.WriteCustomEvent("Unable to get Access Token", traceProps);
                        token = null;
                    }
                }
            }
            
            this.logger.TraceInformation($"Finished {nameof(this.GetAccessTokenForSelectedAccount)} method of {nameof(TokenHelper)}.", traceProps);
            return token;
        }

        /// <inheritdoc/>
        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderFromToken(string userAccessToken)
        {
            if (string.IsNullOrWhiteSpace(userAccessToken))
            {
                throw new ArgumentNullException(nameof(userAccessToken), "Token cannot be empty while fetching Authentication Header value.");
            }

            this.logger.TraceInformation($"Started {nameof(this.GetAuthenticationHeaderFromToken)} method of {nameof(TokenHelper)}.");
            if (userAccessToken.StartsWith($"{ApplicationConstants.BearerAuthenticationScheme.ToLower(CultureInfo.InvariantCulture)} ", System.StringComparison.InvariantCultureIgnoreCase))
            {
                userAccessToken = userAccessToken.Remove(0, 7);
            }

            var resourceToken = await this.GetResourceAccessTokenFromUserToken(userAccessToken).ConfigureAwait(false);
            var authHeader = new AuthenticationHeaderValue(ApplicationConstants.BearerAuthenticationScheme, resourceToken);
            this.logger.TraceInformation($"Finished {nameof(this.GetAuthenticationHeaderFromToken)} method of {nameof(TokenHelper)}.");
            return authHeader;
        }

        /// <summary>
        /// Get an authentication token for the graph resource from the user access token.
        /// </summary>
        /// <param name="userAccessToken">User access token.</param>
        /// <returns>Authentication token for the graph resource defined in the graph provider.</returns>
        private async Task<string> GetResourceAccessTokenFromUserToken(string userAccessToken)
        {
            this.logger.TraceInformation($"Started {nameof(this.GetResourceAccessTokenFromUserToken)} method of {nameof(TokenHelper)}.");
            try
            {
                var isLocalBuild = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_RUN_FROM_PACKAGE"));
                if (!isLocalBuild)
                {
                    string clientId = this.mSGraphSetting.ClientId;
                    string clientSecret = this.mSGraphSetting.ClientCredential;
                    string authority = string.Format(CultureInfo.InvariantCulture, this.mSGraphSetting.Authority, this.mSGraphSetting.TenantId);
                    string managedIdentity = this.mSGraphSetting.ManagedIdentity;
                    ManagedIdentityClientAssertion managedIdentityClientAssertion = new ManagedIdentityClientAssertion(managedIdentity);
                    var confidentialClientApp = ConfidentialClientApplicationBuilder.Create(clientId)
                        .WithClientAssertion(managedIdentityClientAssertion.GetSignedAssertion)
                        .WithAuthority(authority)
                        .Build();

                    UserAssertion userAssertion = new UserAssertion(userAccessToken, this.mSGraphSetting.UserAssertionType);

                    var result = await confidentialClientApp.AcquireTokenOnBehalfOf(new[] { this.mSGraphSetting.GraphResourceId + "/.default" }, userAssertion)
                                                             .ExecuteAsync()
                    .ConfigureAwait(false);

                    this.logger.TraceInformation($"Finished {nameof(this.GetResourceAccessTokenFromUserToken)} method of {nameof(TokenHelper)}.");
                    return result?.AccessToken;
                }
                else
                {
                    var credential = new DefaultAzureCredential();
                    TokenRequestContext context = new TokenRequestContext(new[] { this.mSGraphSetting.GraphResourceId + "/.default" });
                    var accessToken = await credential.GetTokenAsync(context);
                    return accessToken.Token;
                }
            }
            catch (Exception ex)
            {
                this.logger.TraceError($"An error {ex} occurred in {nameof(this.GetResourceAccessTokenFromUserToken)}");
                throw;
            }
        }
    }
}
