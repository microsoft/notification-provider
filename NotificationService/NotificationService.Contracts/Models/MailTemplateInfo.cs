// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Mail template Information.
    /// </summary>
    public class MailTemplateInfo
    {
        /// <summary>
        /// Gets or sets the template name.
        /// </summary>
        [DataMember(Name = "templateId")]
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the template description.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the template type.
        /// </summary>
        [DataMember(Name = "templateType")]
        public string TemplateType { get; set; }
    }
}
