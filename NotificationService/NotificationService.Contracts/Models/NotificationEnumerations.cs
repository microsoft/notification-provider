// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    /// <summary>
    /// Type of the notification supported by system.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Mail
        /// </summary>
        Mail,

        /// <summary>
        /// Meet
        /// </summary>
        Meet,

        /// <summary>
        /// The web delivery channel
        /// </summary>
        Web,
    }

    /// <summary>
    /// Priority of the notification item.
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// High
        /// </summary>
        High,

        /// <summary>
        /// Log
        /// </summary>
        Low,

        /// <summary>
        /// Normal
        /// </summary>
        Normal,
    }

    /// <summary>
    /// Senstivity of the email notification.
    /// </summary>
    public enum MailSensitivity
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Personal
        /// </summary>
        Personal = 1,

        /// <summary>
        /// Private
        /// </summary>
        Private = 2,

        /// <summary>
        /// Confidential
        /// </summary>
        Confidential = 3,
    }

    /// <summary>
    /// Meeting Recurrence Pattern enum.
    /// </summary>
    public enum MeetingRecurrencePattern
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Daily
        /// </summary>
        Daily = 1,

        /// <summary>
        /// Weekly
        /// </summary>
        Weekly = 2,

        /// <summary>
        /// Monthly
        /// </summary>
        Monthly = 3,

        /// <summary>
        /// Yearly
        /// </summary>
        Yearly = 4,
    }
}
