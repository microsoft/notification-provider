// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Startup file method signature is always non-static.", Scope = "member", Target = "~M:NotificationService.Startup.Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder,Microsoft.AspNetCore.Hosting.IWebHostEnvironment)")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Auto generated code.", Scope = "member", Target = "~M:NotificationService.Startup.#ctor(Microsoft.Extensions.Configuration.IConfiguration)")]
[assembly: SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Auto generated code.", Scope = "type", Target = "~T:NotificationService.Program")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "The model validation will take care of it.", Scope = "member", Target = "~M:NotificationService.Controllers.V1.NotificationsController.PostNotifications(System.String,NotificationService.Contracts.Models.Web.Request.WebNotificationRequestItemsContainer)~System.Threading.Tasks.Task{Microsoft.AspNetCore.Mvc.IActionResult}")]
