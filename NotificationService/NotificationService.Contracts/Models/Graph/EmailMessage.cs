// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using NotificationService.Contracts.Models.Graph;

    /// <summary>
    /// Email Message contract of MS Graph Provider.
    /// </summary>
    [DataContract]
    public class EmailMessage
    {
        /// <summary>
        /// Gets or sets subject of the message.
        /// </summary>
        [DataMember(Name = "subject", IsRequired = true)]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets body of the message.
        /// </summary>
        [DataMember(Name = "body", IsRequired = true)]
        public MessageBody Body { get; set; }

        /// <summary>
        /// Gets or sets "To" recipients of the message.
        /// </summary>
        [DataMember(Name = "toRecipients", IsRequired = true)]
        public List<Recipient> ToRecipients { get; set; }

        /// <summary>
        /// Gets or sets "CC" recipients of the message.
        /// </summary>
        [DataMember(Name = "ccRecipients", IsRequired = true)]
        public List<Recipient> CCRecipients { get; set; }

        /// <summary>
        /// Gets or sets "BCC" recipients of the message.
        /// </summary>
        [DataMember(Name = "bccRecipients", IsRequired = true)]
        public List<Recipient> BCCRecipients { get; set; }

        /// <summary>
        /// Gets or sets recipients of the replies to the message.
        /// </summary>
        [DataMember(Name = "replyTo", IsRequired = false)]
        public List<Recipient> ReplyToRecipients { get; set; }

        /// <summary>
        /// Gets or sets attachments to the message.
        /// </summary>
        [DataMember(Name = "attachments", IsRequired = false)]
        public List<FileAttachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the "From" address of the message.
        /// </summary>
        [DataMember(Name = "from", IsRequired = false)]
        public Recipient FromAccount { get; set; }

        /// <summary>
        /// Gets or sets the "SingleValueExtendedProperties" of the message.
        /// </summary>
        [DataMember(Name = "SingleValueExtendedProperties", IsRequired = false)]
        public List<SingleValueExtendedProperty> SingleValueExtendedProperties { get; set; }

        /// <summary>
        /// Gets or Sets Importance of the notification.
        /// </summary>
        [DataMember(Name = "importance")]
        public ImportanceType Importance { get; set; }
    }
}
