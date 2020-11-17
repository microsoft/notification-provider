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
        /// Normal
        /// </summary>
        Normal,

        /// <summary>
        /// Low
        /// </summary>
        Low,
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
}
