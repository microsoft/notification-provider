// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Web.Request
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using NotificationService.Contracts.Entities.Web;
    using NotificationService.Contracts.Models.Graph;

    /// <summary>
    /// The <see cref="WebNotificationRequestItem"/> class represents web notification input.
    /// </summary>
    /// <seealso cref="NotificationItemBase" />
    public class WebNotificationRequestItem : NotificationItemBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebNotificationRequestItem"/> class.
        /// </summary>
        public WebNotificationRequestItem()
        {
            this.Priority = NotificationPriority.Normal;
            this.TrackingId = string.Empty;
        }

        /// <inheritdoc cref="NotificationItemBase"/>
        public override NotificationType NotifyType => NotificationType.Web;

        /// <summary>
        /// Gets or sets the notification title.
        /// </summary>
        /// <value>
        /// The notification title.
        /// </value>
        [DataMember(Name = "title")]
        [Required(ErrorMessage = "The notification title is mandatory.", AllowEmptyStrings = false)]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the notification body.
        /// </summary>
        /// <value>
        /// The notification body.
        /// </value>
        [DataMember(Name = "body")]
        [Required(ErrorMessage = "The notification body is mandatory.", AllowEmptyStrings = false)]
        [StringLength(1000, ErrorMessage = "Title cannot exceed 1000 characters.")]
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
        /// Gets or sets the notification recipient.
        /// </summary>
        /// <value>
        /// The notification recipient.
        /// </value>
        [DataMember(Name = "recipient")]
        [Required(ErrorMessage = "The recipient is mandatory.")]
        public Person Recipient { get; set; }

        /// <summary>
        /// Gets or sets the notification sender.
        /// </summary>
        /// <value>
        /// The notification sender.
        /// </value>
        [DataMember(Name = "sender")]
        [Required(ErrorMessage = "The sender is mandatory.")]
        public Person Sender { get; set; }

        /// <summary>
        /// Gets or sets the notification publish UTC date.
        /// </summary>
        /// <value>
        /// The notification publish UTC date.
        /// </value>
        [Required(ErrorMessage = "The publish date is mandatory.")]
        [DataMember(Name = "publishOnUTCDate")]
        public DateTime PublishOnUTCDate { get; set; }

        /// <summary>
        /// Gets or sets the notification expires on date (UTC).
        /// </summary>
        /// <value>
        /// The notification expires on date (UTC).
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
        /// Converts to <see cref="WebNotificationItemEntity"/> object.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>The instance of <see cref="WebNotificationItemEntity"/>.</returns>
        /// <exception cref="ArgumentException">The application name is mandatory.</exception>
        public WebNotificationItemEntity ToEntity(string applicationName)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("The application name is mandatory.", nameof(applicationName));
            }

            return new WebNotificationItemEntity
            {
                Application = applicationName,
                Title = this.Title,
                Body = this.Body,
                Properties = this.Properties,
                Recipient = this.Recipient,
                Sender = this.Sender,
                PublishOnUTCDate = this.PublishOnUTCDate,
                SendOnUtcDate = this.SendOnUtcDate,
                ExpiresOnUTCDate = this.ExpiresOnUTCDate,
                Status = NotificationItemStatus.Processing,
                Id = Guid.NewGuid().ToString(),
                NotificationId = this.NotificationId ?? Guid.NewGuid().ToString(),
                TrackingId = this.TrackingId,
                Priority = this.Priority,
                AppNotificationType = this.AppNotificationType,
                DeliveredOnChannel = new Dictionary<NotificationDeliveryChannel, bool>
                {
                    { NotificationDeliveryChannel.Web, false },
                    { NotificationDeliveryChannel.Email, false },
                    { NotificationDeliveryChannel.ActionCenter, false },
                },
            };
        }
    }
}
