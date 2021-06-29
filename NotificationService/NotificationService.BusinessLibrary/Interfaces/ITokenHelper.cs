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
        /// Creates an authentication token for the graph resource from the user access token.
        /// </summary>
        /// <returns>Authentication token for the graph resource defined in the graph provider.</returns>
        Task<AuthenticationHeaderValue> GetAuthenticationHeader();
    }
}
