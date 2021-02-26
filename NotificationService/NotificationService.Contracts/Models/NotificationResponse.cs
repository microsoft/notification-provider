// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Response to the Send Notification Requests.
    /// </summary>
    [DataContract]
    public class NotificationResponse
    {
        /// <summary>
        /// Gets or sets Notification Id.
        /// </summary>
        [DataMember(Name = "NotificationId")]
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or sets Tracking Id.
        /// </summary>
        [DataMember(Name = "TrackingId")]
        public string TrackingId { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        [DataMember(Name = "Status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NotificationItemStatus Status { get; set; }

        /// <summary>
        /// Gets or sets ErrorMessage, if notification failed.
        /// </summary>
        [DataMember(Name = "ErrorMessage")]
        public string ErrorMessage { get; set; }
    }
}
