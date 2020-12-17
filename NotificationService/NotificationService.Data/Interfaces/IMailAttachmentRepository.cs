// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts;

    /// <summary>
    /// Repository interface for all Mail Attachment Items.
    /// </summary>
    public interface IMailAttachmentRepository
    {
        /// <summary>
        /// Upload Attachment to the storage provider.
        /// </summary>
        /// <param name="emailNotificationItemEntities">The Entities to read the attachment content from.</param>
        /// <param name="notificationType">Notification Type to create path.</param>
        /// <param name="applicationName">ApplicationName to create container.</param>
        /// <returns>Updated Notification Entities with the Attachment Reference.</returns>
        Task<IList<EmailNotificationItemEntity>> UploadAttachment(IList<EmailNotificationItemEntity> emailNotificationItemEntities, string notificationType, string applicationName);

        /// <summary>
        /// Download Attachment from the storage provider from the reference.
        /// </summary>
        /// <param name="emailNotificationItemEntities">Entities to get the reference from.</param>
        /// <param name="applicationName">The applicationName to get the container.</param>
        /// <returns>Updated entities with attachments.</returns>
        Task<IList<EmailNotificationItemEntity>> DownloadAttachment(IList<EmailNotificationItemEntity> emailNotificationItemEntities, string applicationName);
    }
}
