// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.FunctionalTests
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
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
            var authContext = new AuthenticationContext(this.Configuration[FunctionalConstants.Authority]);
            var authResult = await authContext.AcquireTokenAsync(this.Configuration[FunctionalConstants.ClientId], new ClientCredential(this.Configuration[FunctionalConstants.ClientId], this.Configuration[FunctionalConstants.ClientSecret]));
            return authResult.AccessToken;
        }
    }
}
