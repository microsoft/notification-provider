// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.GDPR
{
    /// <summary>
    /// Notification queue base entity item.
    /// </summary>
    public class NotificationQueueItem
    {
        /// <summary>
        /// Gets or Sets NotificaitonId.
        /// </summary>
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or Sets From field.
        /// </summary>
        public string From { get; set; }
    }
}
