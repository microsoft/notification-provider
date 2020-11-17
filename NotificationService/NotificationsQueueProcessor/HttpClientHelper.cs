// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationsQueueProcessor
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// HttpClientHelper.
    /// </summary>
    /// <seealso cref="NotificationsQueueProcessor.IHttpClientHelper" />
    public class HttpClientHelper : IHttpClientHelper
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientHelper"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        public HttpClientHelper(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            string httpTimeOut = $"{Environment.GetEnvironmentVariable(Constants.EnvVariableHttpTimeOutInSec)}";
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
            string bearerToken = await this.HttpAuthenticationAsync(Environment.GetEnvironmentVariable(Constants.EnvVariableAuthority), Environment.GetEnvironmentVariable(Constants.EnvVariableClientId));
            if (bearerToken != null)
            {
                this.httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
                return await this.httpClient.PostAsync(url, content).ConfigureAwait(false);
            }
            else
            {
                throw new Exception($"Unable to generate token for {Environment.GetEnvironmentVariable(Constants.EnvVariableClientId)} in ProcessNotificationQueueItem");
            }
        }

        private async Task<string> HttpAuthenticationAsync(string authority, string clientId)
        {
            var authContext = new AuthenticationContext(authority);
            var authResult = await authContext.AcquireTokenAsync(clientId, new ClientCredential(clientId, Environment.GetEnvironmentVariable(Constants.EnvVariableClientSecret)));
            return authResult.AccessToken;
        }
    }
}
