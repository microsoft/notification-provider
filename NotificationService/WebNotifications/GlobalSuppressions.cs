// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The background service must only log the errors to keep itself running.", Scope = "member", Target = "~M:WebNotifications.BackgroundServices.NotificationsCarrierService.ExecuteAsync(System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The carrier must only log the errors.", Scope = "member", Target = "~M:WebNotifications.Carriers.WebNotificationsCarrier.SendInternalAsync(System.Collections.Generic.IEnumerable{NotificationService.Contracts.Models.Web.Response.WebNotification})~System.Threading.Tasks.Task{System.Collections.Generic.List{System.String}}")]
