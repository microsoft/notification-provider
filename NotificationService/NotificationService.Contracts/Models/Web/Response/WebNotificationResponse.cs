// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Web.Response
{
    using System.Collections.Generic;

    /// <summary>
    /// The <see cref="WebNotificationResponse"/> class represents the notifications response contract.
    /// </summary>
    public class WebNotificationResponse
    {
        /// <summary>
        /// Gets the notifications.
        /// </summary>
        /// <value>
        /// The notifications.
        /// </value>
        public List<WebNotification> Notifications { get; } = new List<WebNotification>();
    }
}
