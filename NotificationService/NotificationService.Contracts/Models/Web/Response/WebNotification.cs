// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Web.Response
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using NotificationService.Contracts.Entities.Web;
    using NotificationService.Contracts.Models.Graph;

    /// <summary>
    /// The <see cref="WebNotification"/> class stores the web notification information to be shared in response.
    /// </summary>
    [DataContract]
    public class WebNotification
    {
        /// <summary>
        /// Gets or sets the notification identifier.
        /// </summary>
        /// <value>
        /// The notification identifier.
        /// </value>
        [DataMember(Name = "notificationId")]
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the notification title.
        /// </summary>
        /// <value>
        /// The notification title.
        /// </value>
        [DataMember(Name = "title", IsRequired = true)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the notification body.
        /// </summary>
        /// <value>
        /// The notification body.
        /// </value>
        [DataMember(Name = "body", IsRequired = true)]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the notification properties.
        /// </summary>
        /// <value>
        /// The notification properties.
        /// </value>
        [DataMember(Name = "properties")]
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Gets or sets the notification priority.
        /// </summary>
        /// <value>
        /// The notification priority.
        /// </value>
        [DataMember(Name = "priority")]
        public NotificationPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the notification sender.
        /// </summary>
        /// <value>
        /// The notification sender.
        /// </value>
        [DataMember(Name = "sender", IsRequired = true)]
        public Person Sender { get; set; }

        /// <summary>
        /// Gets or sets the notification read status.
        /// </summary>
        /// <value>
        /// The notification read status.
        /// </value>
        [DataMember(Name = "readStatus", IsRequired = true)]
        public NotificationReadStatus ReadStatus { get; set; }

        /// <summary>
        /// Gets or sets the notification publish UTC date.
        /// </summary>
        /// <value>
        /// The notification publish UTC date.
        /// </value>
        [DataMember(Name = "publishOnUTCDate")]
        public DateTime PublishOnUTCDate { get; set; }

        /// <summary>
        /// Gets or sets the notification expiry UTC date.
        /// </summary>
        /// <value>
        /// The notification expiry UTC date.
        /// </value>
        [DataMember(Name = "expiresOnUTCDate")]
        public DateTime? ExpiresOnUTCDate { get; set; }

        /// <summary>
        /// Gets or sets the application defined notification type.
        /// </summary>
        /// <value>
        /// The application defined notification type.
        /// </value>
        [DataMember(Name = "appNotificationType")]
        public string AppNotificationType { get; set; }

        /// <summary>
        /// Gets or sets the notification recipient (non-serialized).
        /// </summary>
        /// <value>
        /// The notification recipient.
        /// </value>
        [JsonIgnore]
        public Person Recipient { get; set; }

        /// <summary>
        /// Gets or sets the application name (non-serialized).
        /// </summary>
        /// <value>
        /// The application name.
        /// </value>
        [JsonIgnore]
        public string Application { get; set; }
    }
}
