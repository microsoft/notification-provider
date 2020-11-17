// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// Attachment to a notification item.
    /// </summary>
    [DataContract]
    public class NotificationAttachment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationAttachment"/> class.
        /// </summary>
        public NotificationAttachment()
        {
            this.IsInline = false;
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        [DataMember(Name = "fileName")]
        [Required(ErrorMessage = "File name is mandatory for notification attachment.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file base64.
        /// </summary>
        [DataMember(Name = "fileBase64")]
        [Required(ErrorMessage = "Base64 content is mandatory for notification attachment.")]
        public string FileBase64 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is inline.
        /// </summary>
        [DataMember(Name = "isInline")]
        public bool IsInline { get; set; }
    }
}
