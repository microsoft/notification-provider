// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.GDPR
{
    /// <summary>
    /// Email Notification queue item entity.
    /// </summary>
    public class EmailNotificationQueueItem : NotificationQueueItem
    {
        /// <summary>
        /// Gets or Sets To.
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Gets or Sets CC.
        /// </summary>
        public string CC { get; set; }

        /// <summary>
        /// Gets or Sets BCC.
        /// </summary>
        public string BCC { get; set; }
    }
}
