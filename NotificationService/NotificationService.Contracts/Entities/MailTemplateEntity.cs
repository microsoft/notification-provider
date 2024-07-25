// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Entities
{
    using System.Runtime.Serialization;
    using NotificationService.Contracts.Models;
    using Azure.Data.Tables;

    /// <summary>
    /// Mail template entity.
    /// </summary>
    [DataContract]
    public class MailTemplateEntity : TableEntityBase
    {
        /// <summary>
        /// Gets or sets the template name.
        /// </summary>
        [DataMember(Name = "TemplateId")]
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the template description.
        /// </summary>
        [DataMember(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the template content.
        /// </summary>
        [DataMember(Name = "Content")]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the template type.
        /// </summary>
        [DataMember(Name = "TemplateType")]
        public string TemplateType { get; set; }

        /// <summary>
        /// Gets or sets Application associated to the mail template item.
        /// </summary>
        [DataMember(Name = "Application")]
        public string Application { get; set; }
    }
}
