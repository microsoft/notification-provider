// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using NotificationService.Contracts;

    /// <summary>
    /// Contract for the notification item in the queue.
    /// </summary>
    public class QueueNotificationItem
    {
        /// <summary>
        /// Gets or sets unique identifiers of the notification items.
        /// </summary>
        public string[] NotificationIds { get; set; }

        /// <summary>
        /// Gets or sets the name of application associate to the notification item.
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        /// Gets or sets the type of the notification item - email or meeting invite.
        /// </summary>
        public NotificationType NotificationType { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        /// <value>
        /// The correlation identifier.
        /// </value>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the IgnoreAlreadySent flag is set.
        /// </summary>
        public bool IgnoreAlreadySent { get; set; }
    }
}
