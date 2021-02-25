// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using NotificationService.Contracts;

    /// <summary>
    /// Helper class to handle token related activities.
    /// </summary>
    public interface ITokenHelper
    {
        /// <summary>
        /// Fetches the token for the selected account credential.
        /// </summary>
        /// <param name="selectedAccountCredential">Account credential.</param>
        /// <returns>Token value.</returns>
        Task<string> GetAccessTokenForSelectedAccount(AccountCredential selectedAccountCredential);

        /// <summary>
        /// Creates an authentication token for the graph resource from the user access token.
        /// </summary>
        /// <param name="userAccessToken">User access token.</param>
        /// <returns>Authentication token for the graph resource defined in the graph provider.</returns>
        Task<AuthenticationHeaderValue> GetAuthenticationHeaderFromToken(string userAccessToken);

        /// <summary>
        /// return the authentication header for selected account.
        /// </summary>
        /// <param name="selectedAccountCredential">selectedAccountCredential.</param>
        /// <returns>AuthenticationHeaderValue.</returns>
        Task<AuthenticationHeaderValue> GetAuthenticationHeaderValueForSelectedAccount(AccountCredential selectedAccountCredential);
    }
}
