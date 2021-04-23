// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationsQueueProcessor
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using NotificationService.Common;

    /// <summary>
    /// HttpClientHelper.
    /// </summary>
    /// <seealso cref="NotificationsQueueProcessor.IHttpClientHelper" />
    public class HttpClientHelper : IHttpClientHelper
    {
        private readonly HttpClient httpClient;

        // Instance of Application Configuration.
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientHelper"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="configuration">The configuration.</param>
        public HttpClientHelper(HttpClient httpClient, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
            string httpTimeOut = this.configuration?[Constants.HttpTimeOutInSec];
            if (int.TryParse(httpTimeOut, out int timeOut))
            {
                this.httpClient.Timeout = TimeSpan.FromSeconds(timeOut);
            }
        }

        /// <summary>
        /// Posts the asynchronous.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="content">The content.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            string bearerToken = await this.HttpAuthenticationAsync(this.configuration?[Constants.Authority], this.configuration?[Constants.ClientId]);
            if (bearerToken != null)
            {
                this.httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(ApplicationConstants.BearerAuthenticationScheme, bearerToken);
                return await this.httpClient.PostAsync(url, content).ConfigureAwait(false);
            }
            else
            {
                throw new Exception($"Unable to generate token for {this.configuration?[Constants.ClientId]} in ProcessNotificationQueueItem");
            }
        }

        private async Task<string> HttpAuthenticationAsync(string authority, string clientId)
        {
            var authContext = new AuthenticationContext(authority);
            var authResult = await authContext.AcquireTokenAsync(clientId, new ClientCredential(clientId, this.configuration?[Constants.ClientSecret]));
            return authResult.AccessToken;
        }
    }
}
