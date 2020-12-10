// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    /// <summary>
    /// Status of a Notification Item.
    /// </summary>
    public enum NotificationItemStatus
    {
        /// <summary>
        /// Queued
        /// </summary>
        Queued,

        /// <summary>
        /// Processing
        /// </summary>
        Processing,

        /// <summary>
        /// Retrying
        /// </summary>
        Retrying,

        /// <summary>
        /// Failed
        /// </summary>
        Failed,

        /// <summary>
        /// Sent
        /// </summary>
        Sent,

        /// <summary>
        /// If MailOn is false, notification is considered as fake
        /// </summary>
        FakeMail,

        /// <summary>
        /// Invalid Notification.
        /// </summary>
        Invalid,

        /// <summary>
        /// Not Queued.
        /// </summary>
        NotQueued,
    }
}
