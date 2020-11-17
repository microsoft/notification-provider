// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Notification Attachment Entity.
    /// </summary>
    [DataContract]
    public class NotificationAttachmentEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationAttachmentEntity"/> class.
        /// </summary>
        public NotificationAttachmentEntity()
        {
            this.IsInline = false;
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        [DataMember(Name = "FileName")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file base64.
        /// </summary>
        [DataMember(Name = "FileBase64")]
        public string FileBase64 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is inline.
        /// </summary>
        [DataMember(Name = "IsInline")]
        public bool IsInline { get; set; }
    }
}
