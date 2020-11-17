// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// Response to the Send Batch Item Notification Requests.
    /// </summary>
    [DataContract]
    public class NotificationBatchItemResponse
    {
        /// <summary>
        /// Gets or sets Notification Id.
        /// </summary>
        [DataMember(Name = "NotificationId")]
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        [DataMember(Name = "Status")]
        public HttpStatusCode Status { get; set; }

        /// <summary>
        /// Gets or sets Error details if the item failed to be sent.
        /// </summary>
        [DataMember(Name = "Error")]
        public string Error { get; set; }
    }
}
