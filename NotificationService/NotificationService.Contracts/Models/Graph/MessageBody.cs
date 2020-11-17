// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Body of MS Graph Message.
    /// </summary>
    [DataContract]
    public class MessageBody
    {
        /// <summary>
        /// Gets or sets the content of the message body.
        /// </summary>
        [DataMember(Name = "content", IsRequired = true)]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the content type of the message body.
        /// </summary>
        [DataMember(Name = "contentType", IsRequired = false)]
        public string ContentType { get; set; } = "Text";
    }
}
