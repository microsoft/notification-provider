// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base class for Notification Items.
    /// </summary>
    [DataContract]
    public abstract class NotificationItemBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationItemBase"/> class.
        /// </summary>
        protected NotificationItemBase()
        {
            this.SendOnUtcDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the Notification Id.
        /// </summary>
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets the type of the notify.
        /// </summary>
        [DataMember(Name = "notifType")]
        [Required(ErrorMessage = "Notification type is mandatory for a notification.")]
        public abstract NotificationType NotifyType { get; }

        /// <summary>
        /// Gets or sets the send/received on UTC date.
        /// </summary>
        [DataMember(Name = "sendOnUtcDate")]
        public DateTime SendOnUtcDate { get; set; }

        /// <summary>
        /// Gets or sets the TrackingId.
        /// </summary>
        [DataMember(Name = "trackingId")]
        public string TrackingId { get; set; }
    }
}
