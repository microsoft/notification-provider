// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Entities.Web
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using NotificationService.Contracts.Models.Graph;
    using NotificationService.Contracts.Models.Web.Response;

    /// <summary>
    /// The <see cref="WebNotificationItemEntity"/> class stores the web notification information.
    /// </summary>
    /// <seealso cref="NotificationItemBaseEntity" />
    public class WebNotificationItemEntity : NotificationItemBaseEntity
    {
        /// <inheritdoc cref="NotificationItemBaseEntity" />
        public override NotificationType NotifyType => NotificationType.Web;

        /// <summary>
        /// Gets or sets the notification priority.
        /// </summary>
        /// <value>
        /// The notification priority.
        /// </value>
        [DataMember(Name = "Priority")]
        public NotificationPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        [DataMember(Name = "Application")]
        public string Application { get; set; }

        /// <summary>
        /// Gets or sets the notification title.
        /// </summary>
        /// <value>
        /// The notification title.
        /// </value>
        [DataMember(Name = "Title", IsRequired = true)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the notification body.
        /// </summary>
        /// <value>
        /// The notification body.
        /// </value>
        [DataMember(Name = "Body", IsRequired = true)]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the notification properties.
        /// </summary>
        /// <value>
        /// The notification properties.
        /// </value>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Gets or sets the notification recipient.
        /// </summary>
        /// <value>
        /// The notification recipient.
        /// </value>
        [DataMember(Name = "Recipient", IsRequired = true)]
        public Person Recipient { get; set; }

        /// <summary>
        /// Gets or sets the notification sender.
        /// </summary>
        /// <value>
        /// The notification sender.
        /// </value>
        [DataMember(Name = "Sender", IsRequired = true)]
        public Person Sender { get; set; }

        /// <summary>
        /// Gets or sets the notification read status.
        /// </summary>
        /// <value>
        /// The notification read status.
        /// </value>
        [DataMember(Name = "ReadStatus", IsRequired = true)]
        public NotificationReadStatus ReadStatus { get; set; } = NotificationReadStatus.New;

        /// <summary>
        /// Gets or sets the notification publish UTC date.
        /// </summary>
        /// <value>
        /// The notification publish UTC date.
        /// </value>
        [DataMember(Name = "PublishOnUTCDate")]
        public DateTime PublishOnUTCDate { get; set; }

        /// <summary>
        /// Gets or sets the notification expiry UTC date.
        /// </summary>
        /// <value>
        /// The notification expiry UTC date.
        /// </value>
        [DataMember(Name = "ExpiresOnUTCDate")]
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
        /// Gets or sets the notification delivered on channel mapping.
        /// </summary>
        /// <value>
        /// The notification delivered on channel mapping.
        /// </value>
        [DataMember(Name = "DeliveredOnChannel")]
        public Dictionary<NotificationDeliveryChannel, bool> DeliveredOnChannel { get; set; }

        /// <summary>
        /// Converts to <see cref="WebNotification"/>.
        /// </summary>
        /// <returns>The instance of <see cref="WebNotification"/>.</returns>
        public WebNotification ToWebNotification()
        {
            return new WebNotification
            {
                NotificationId = this.NotificationId,
                Title = this.Title,
                Body = this.Body,
                Properties = this.Properties,
                Priority = this.Priority,
                Sender = this.Sender,
                Recipient = this.Recipient,
                ReadStatus = this.ReadStatus,
                PublishOnUTCDate = this.PublishOnUTCDate,
                ExpiresOnUTCDate = this.ExpiresOnUTCDate,
                Application = this.Application,
                AppNotificationType = this.AppNotificationType,
            };
        }
    }
}
