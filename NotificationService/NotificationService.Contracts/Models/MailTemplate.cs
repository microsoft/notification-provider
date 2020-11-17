// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Mail template contract.
    /// </summary>
    public class MailTemplate
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
    }
}
