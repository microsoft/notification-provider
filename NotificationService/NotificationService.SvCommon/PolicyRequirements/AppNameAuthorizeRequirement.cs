// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.SvCommon
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Authorization Requirement to validate if the request contains a valid Application name.
    /// </summary>
    public class AppNameAuthorizeRequirement : IAuthorizationRequirement
    {
    }
}
