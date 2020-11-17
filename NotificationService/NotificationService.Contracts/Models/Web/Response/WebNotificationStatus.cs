// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Web.Response
{
    using System.Collections.Generic;
    using NotificationService.Contracts.Entities.Web;

    /// <summary>
    /// The <see cref="WebNotificationStatus"/> class stores the notification status information.
    /// </summary>
    public class WebNotificationStatus
    {
        /// <summary>
        /// Gets or sets the tracking identifier.
        /// </summary>
        /// <value>
        /// The tracking identifier.
        /// </value>
        public string TrackingId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tracking identifier is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the tracking identifier is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValidTrackingId { get; set; }

        /// <summary>
        /// Gets or sets the notification identifier.
        /// </summary>
        /// <value>
        /// The notification identifier.
        /// </value>
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets the notification delivery status.
        /// </summary>
        /// <value>
        /// The delivery status.
        /// </value>
        public Dictionary<string, bool> DeliveryStatus { get; } = new Dictionary<string, bool>();

        /// <summary>
        /// Gets or sets the read status.
        /// </summary>
        /// <value>
        /// The read status.
        /// </value>
        public NotificationReadStatus ReadStatus { get; set; }
    }
}
