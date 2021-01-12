﻿namespace NotificationService.Contracts.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// BlobEmailData.
    /// </summary>
    public class BlobEmailData
    {
        /// <summary>
        /// Gets or sets notificationId.
        /// </summary>
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or Sets Email Body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or Sets Attachments.
        /// </summary>
        public IEnumerable<NotificationAttachmentEntity> Attachments { get; set; }

        /// <summary>
        /// Gets or sets templateData.
        /// </summary>
        public string TemplateData { get; set; }
    }
}
