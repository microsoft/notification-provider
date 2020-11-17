// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Entities
{
    using System.Runtime.Serialization;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Mail template entity.
    /// </summary>
    [DataContract]
    public class MailTemplateEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the template name.
        /// </summary>
        [DataMember(Name = "templateName")]
        public string TemplateName { get; set; }

        /// <summary>
        /// Gets or sets the template description.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the template content.
        /// </summary>
        [DataMember(Name = "content")]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the template type.
        /// </summary>
        [DataMember(Name = "templateType")]
        public string TemplateType { get; set; }

        /// <summary>
        /// Gets or sets Application associated to the mail template item.
        /// </summary>
        [DataMember(Name = "Application")]
        public string Application { get; set; }
    }
}
