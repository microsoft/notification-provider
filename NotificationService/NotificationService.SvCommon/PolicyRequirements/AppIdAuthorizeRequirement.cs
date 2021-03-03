// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.SvCommon
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Authorization Requirement to validate the request with Application Valid Audiences.
    /// </summary>
    public class AppIdAuthorizeRequirement : IAuthorizationRequirement
    {
    }
}
