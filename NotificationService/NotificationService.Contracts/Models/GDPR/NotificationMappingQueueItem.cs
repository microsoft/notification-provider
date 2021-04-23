// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.GDPR
{
    /// <summary>
    /// Class for adding notification to queue for creating email-notification mapping.
    /// </summary>
    public class NotificationMappingQueueItem
    {
        /// <summary>
        /// Gets or Sets Notification Type.
        /// </summary>
        public string NotificationType { get; set; }

        /// <summary>
        /// Gets or Sets payload.
        /// </summary>
        public dynamic Payload { get; set; }

        /// <summary>
        /// Gets or Sets Application Name.
        /// </summary>
        public string ApplicationName { get; set; }
    }
}
