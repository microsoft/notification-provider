// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System;
    using System.Runtime.Serialization;
    using Azure.Data.Tables;

    /// <summary>
    /// Base class for Notification Items.
    /// </summary>
    [DataContract]
    public abstract class NotificationItemBaseEntity : CosmosDBEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationItemBaseEntity"/> class.
        /// </summary>
        protected NotificationItemBaseEntity()
        {
            this.SendOnUtcDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets Unique Identifier for the Notification Item.
        /// </summary>
        [DataMember(Name = "NotificationId")]
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets the type of the notify.
        /// </summary>
        [DataMember(Name = "NotifyType")]
        public abstract NotificationType NotifyType { get; }

        /// <summary>
        /// Gets or sets the send on UTC date.
        /// </summary>
        [DataMember(Name = "SendOnUtcDate")]
        public DateTime SendOnUtcDate { get; set; }

        /// <summary>
        /// Gets or sets the TrackingId.
        /// </summary>
        [DataMember(Name = "TrackingId")]
        public string TrackingId { get; set; }

        /// <summary>
        /// Gets or sets status of the Notification item.
        /// </summary>
        [DataMember(Name = "Status")]
        public NotificationItemStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a counter value on how many attempts made to send the notification item.
        /// </summary>
        [DataMember(Name = "TryCount")]
        public int TryCount { get; set; }

        /// <summary>
        /// Gets or sets error message when the processing failed.
        /// </summary>
        [DataMember(Name = "ErrorMessage")]
        public string ErrorMessage { get; set; }
    }
}
