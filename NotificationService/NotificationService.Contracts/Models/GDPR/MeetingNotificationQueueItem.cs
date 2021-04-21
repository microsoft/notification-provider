// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.GDPR
{
    /// <summary>
    /// Meeting Notification queue item entity.
    /// </summary>
    public class MeetingNotificationQueueItem : NotificationQueueItem
    {
        /// <summary>
        /// Gets or Sets RequiredAttendees.
        /// </summary>
        public string RequiredAttendees { get; set; }

        /// <summary>
        /// Gets or Sets OptionalAttendees.
        /// </summary>
        public string OptionalAttendees { get; set; }
    }
}
