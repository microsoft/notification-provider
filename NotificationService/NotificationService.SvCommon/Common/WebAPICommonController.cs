// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.SvCommon.Common
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.AspNetCore.Mvc;
    using NotificationService.Common;

    /// <summary>
    /// Common Base Controller for Web APIs.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class WebAPICommonController : Controller
    {
        /// <summary>
        /// Fetches the token portion from the Authorization header of Request.
        /// </summary>
        /// <returns>Token passed to the request.</returns>
        protected string GetTokenFromRequest()
        {
            var authorizationToken = this.Request?.Headers?[ApplicationConstants.AuthorizationHeaderName].Count > 0 ? this.Request.Headers[ApplicationConstants.AuthorizationHeaderName][0] : null;
            if (!string.IsNullOrEmpty(authorizationToken) && authorizationToken.StartsWith($"{ApplicationConstants.BearerAuthenticationScheme.ToLower(CultureInfo.InvariantCulture)} ", System.StringComparison.InvariantCultureIgnoreCase))
            {
                authorizationToken = authorizationToken.Remove(0, 7);
            }

            return authorizationToken;
        }
    }
}
