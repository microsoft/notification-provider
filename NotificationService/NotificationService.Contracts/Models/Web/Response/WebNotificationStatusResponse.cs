// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Web.Response
{
    using System.Collections.Generic;

    /// <summary>
    /// The <see cref="WebNotificationStatusResponse"/> class represents the notifications status response contract.
    /// </summary>
    public class WebNotificationStatusResponse
    {
        /// <summary>
        /// Gets the notification statuses.
        /// </summary>
        /// <value>
        /// The notification statuses.
        /// </value>
        public List<WebNotificationStatus> NotificationStatus { get; } = new List<WebNotificationStatus>();
    }
}
