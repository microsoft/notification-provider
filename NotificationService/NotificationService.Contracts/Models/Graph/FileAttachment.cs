// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// File Attachment of MS Graph Message.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DataContract]
    public class FileAttachment
    {
        /// <summary>
        /// Gets the attachment type.
        /// </summary>
        [DataMember(Name = "@odata.type")]
        public string Type { get; } = "#microsoft.graph.fileAttachment";

        /// <summary>
        /// Gets or sets base-64 encoded contents of the file.
        /// </summary>
        [DataMember(Name = "contentBytes", IsRequired = true)]
        public string ContentBytes { get; set; }

        /// <summary>
        /// Gets or sets name of the file.
        /// </summary>
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this an inline attachment or not.
        /// </summary>
        [DataMember(Name = "isInline", IsRequired = false)]
        public bool IsInline { get; set; } = false;
    }
}
