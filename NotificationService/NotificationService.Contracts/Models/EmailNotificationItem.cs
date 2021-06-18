// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using NotificationService.Common.CustomValidations;

    /// <summary>
    /// Email notification item.
    /// </summary>
    [DataContract]
    public class EmailNotificationItem : NotificationItemBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailNotificationItem"/> class.
        /// </summary>
        public EmailNotificationItem()
        {
            this.Priority = NotificationPriority.Normal;
            this.Attachments = Array.Empty<NotificationAttachment>();
            this.SendOnUtcDate = DateTime.UtcNow;
            this.TrackingId = string.Empty;
        }

        /// <summary>
        /// Gets the type of the notify.
        /// </summary>
        [DataMember(Name = "notifyType")]
        public override NotificationType NotifyType
        {
            get { return NotificationType.Mail; }
        }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        [DataMember(Name = "priority")]
        public NotificationPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets from.
        /// </summary>
        [DataMember(Name = "from")]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets to.
        /// </summary>
        [DataMember(Name = "to")]
        [Required(ErrorMessage = "'To' recipients is mandatory for email notifications.")]
        [EmailIdListValidation(PropertyName = "To", Nullable = false)]
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the cc.
        /// </summary>
        [DataMember(Name = "cc")]
        [EmailIdListValidation(PropertyName = "CC", Nullable = true)]
        public string CC { get; set; }

        /// <summary>
        /// Gets or sets the BCC.
        /// </summary>
        [DataMember(Name = "bcc")]
        [EmailIdListValidation(PropertyName = "BCC", Nullable = true)]
        public string BCC { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        [DataMember(Name = "subject")]
        [Required(ErrorMessage = "Subject is mandatory for email notifications.")]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        [DataMember(Name = "body")]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the attachments.
        /// </summary>
        [DataMember(Name = "attachments")]
        public IEnumerable<NotificationAttachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the sensitivity.
        /// </summary>
        [DataMember(Name = "sensitivity")]
        public MailSensitivity Sensitivity { get; set; }

        /// <summary>
        /// Gets or sets the Reply to addresses.
        /// </summary>
        [DataMember(Name = "replyTo")]
        public string ReplyTo { get; set; }

        /// <summary>
        /// Gets or sets the TemplateId.
        /// </summary>
        [DataMember(Name = "templateId")]
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the TemplateData.
        /// </summary>
        [DataMember(Name = "templateData")]
        public string TemplateData { get; set; }
    }
}
