// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.FunctionalTests
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Client;
    using System.Threading.Tasks;

    public class TokenUtility
    {
        private IConfiguration Configuration;

        public TokenUtility(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }


        public async Task<string> GetTokenAsync()
        {
            var app = ConfidentialClientApplicationBuilder.Create(this.Configuration[FunctionalConstants.ClientId])
            .WithClientSecret(this.Configuration[FunctionalConstants.ClientSecret])
            .WithAuthority(this.Configuration[FunctionalConstants.Authority])
            .Build();

            var authResult = await app.AcquireTokenForClient(
            new[] { $"{this.Configuration[FunctionalConstants.ClientId]}/.default" })
            .ExecuteAsync()
            .ConfigureAwait(false);
            return authResult.AccessToken;
        }
    }
}
