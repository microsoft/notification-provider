// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Email Notification Entity.
    /// </summary>
    [DataContract]
    public class EmailNotificationItemEntity : NotificationItemBaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailNotificationItemEntity"/> class.
        /// </summary>
        public EmailNotificationItemEntity()
        {
            this.Priority = NotificationPriority.Normal;
            this.Attachments = Array.Empty<NotificationAttachmentEntity>();
            this.SendOnUtcDate = DateTime.UtcNow;
            this.TrackingId = string.Empty;
        }

        /// <summary>
        /// Gets the type of the notify.
        /// </summary>
        public override NotificationType NotifyType
        {
            get { return NotificationType.Mail; }
        }

        /// <summary>
        /// Gets or sets Application associated to the notification item.
        /// </summary>
        [DataMember(Name = "Application")]
        public string Application { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        [DataMember(Name = "Priority")]
        public NotificationPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets from.
        /// </summary>
        [DataMember(Name = "From")]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets to.
        /// </summary>
        [DataMember(Name = "To")]
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the cc.
        /// </summary>
        [DataMember(Name = "CC")]
        public string CC { get; set; }

        /// <summary>
        /// Gets or sets the BCC.
        /// </summary>
        [DataMember(Name = "BCC")]
        public string BCC { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        [DataMember(Name = "Subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        [DataMember(Name = "Body")]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the attachments.
        /// </summary>
        [DataMember(Name = "Attachments")]
        public IEnumerable<NotificationAttachmentEntity> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the attachment reference.
        /// </summary>
        [DataMember(Name = "AttachmentReference")]
        public string AttachmentReference { get; set; }

        /// <summary>
        /// Gets or sets the Sensitivity.
        /// </summary>
        [DataMember(Name = "Sensitivity")]
        public MailSensitivity Sensitivity { get; set; }

        /// <summary>
        /// Gets or sets the Reply to addresses.
        /// </summary>
        [DataMember(Name = "ReplyTo")]
        public string ReplyTo { get; set; }

        /// <summary>
        /// Gets or sets Mailbox Account used to deliver the email.
        /// </summary>
        [DataMember(Name = "EmailAccountUsed")]
        public string EmailAccountUsed { get; set; }

        /// <summary>
        /// Gets or sets the TemplateId.
        /// </summary>
        [DataMember(Name = "TemplateId")]
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the TemplateData.
        /// </summary>
        [DataMember(Name = "TemplateData")]
        public string TemplateData { get; set; }

        /// <summary>
        /// Gets or sets the TemplateData.
        /// </summary>
        [DataMember(Name = "EmailBodyBlobPath")]
        public string EmailBodyBlobPath { get; set; }
    }
}
