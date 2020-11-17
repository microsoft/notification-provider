// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.SvCommon.Common
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// The base class for controllers requiring bearer token authorization.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class WebAPIAuthenticatedController : WebAPICommonController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebAPIAuthenticatedController" /> class.
        /// </summary>
        public WebAPIAuthenticatedController()
        {
        }
    }
}
