// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Email Notification Item CosmosDb Entity.
    /// </summary>
    [DataContract]
    public class EmailNotificationItemCosmosDbEntity : CosmosDBEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailNotificationItemCosmosDbEntity"/> class.
        /// </summary>
        public EmailNotificationItemCosmosDbEntity()
        {
            this.Priority = "Normal";
            this.SendOnUtcDate = DateTime.UtcNow;
            this.TrackingId = string.Empty;
        }

        /// <summary>
        /// Gets the type of the notify.
        /// </summary>
        [DataMember(Name = "NotifyType")]
        public static string NotifyType => "Mail";

        /// <summary>
        /// Gets or sets Application associated to the notification item.
        /// </summary>
        [DataMember(Name = "Application")]
        public string Application { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        [DataMember(Name = "Priority")]
        public string Priority { get; set; }

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
        /// Gets or sets error message when the email processing failed.
        /// </summary>
        [DataMember(Name = "ErrorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the TemplateId.
        /// </summary>
        [DataMember(Name = "TemplateId")]
        public string TemplateId { get; set; }


        /// <summary>
        /// Gets or sets Unique Identifier for the Notification Item.
        /// </summary>
        [DataMember(Name = "NotificationId")]
        public string NotificationId { get; set; }

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
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a counter value on how many attempts made to send the notification item.
        /// </summary>
        [DataMember(Name = "TryCount")]
        public int TryCount { get; set; }

        /// <summary>
        /// Gets or sets the EmailBodyBlobPath.
        /// </summary>
        [DataMember(Name = "EmailBodyBlobPath")]
        public string EmailBodyBlobPath { get; set; }
    }
}
