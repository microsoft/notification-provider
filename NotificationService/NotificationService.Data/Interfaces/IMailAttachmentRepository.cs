// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;

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

        /// <summary>
        /// Upload meeting notifiation attachments.
        /// </summary>
        /// <param name="emailNotificationItemEntities"> meeting notification entities.</param>
        /// <param name="notificationType"> notifiation type to create path.</param>
        /// <param name="applicationName">applicationname to create container.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IList<MeetingNotificationItemEntity>> UploadMeetingAttachments(IList<MeetingNotificationItemEntity> emailNotificationItemEntities, string notificationType, string applicationName);

        /// <summary>
        /// Download meeting attachments from blob.
        /// </summary>
        /// <param name="emailNotificationItemEntities"> Meeting notification entities. </param>
        /// <param name="applicationName"> applincationname as container name. </param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IList<MeetingNotificationItemEntity>> DownloadMeetingAttachment(IList<MeetingNotificationItemEntity> emailNotificationItemEntities, string applicationName);

        /// <summary>
        /// UploadEmail to blob.
        /// </summary>
        /// <param name="emailNotificationItemEntities">List of <see cref="EmailNotificationItemEntity"/>.</param>
        /// <param name="notificationType">notificationType.</param>
        /// <param name="applicationName">applicationName.</param>
        /// <returns>AA List of <see cref="EmailNotificationItemEntity"/>.</returns>
        Task<IList<EmailNotificationItemEntity>> UploadEmail(IList<EmailNotificationItemEntity> emailNotificationItemEntities, string notificationType, string applicationName);

        /// <summary>
        /// Downloads email from blob.
        /// </summary>
        /// <param name="emailNotificationItemEntities">List of <see cref="EmailNotificationItemEntity"/>.</param>
        /// <param name="applicationName">applicationName.</param>
        /// <returns>A List of <see cref="EmailNotificationItemEntity"/>.</returns>
        Task<IList<EmailNotificationItemEntity>> DownloadEmail(IList<EmailNotificationItemEntity> emailNotificationItemEntities, string applicationName);


        /// <summary>
        /// UploadEmail to blob.
        /// </summary>
        /// <param name="meetingNotificationItemEntities">List of <see cref="MeetingNotificationItemEntity"/>.</param>
        /// <param name="notificationType">notificationType.</param>
        /// <param name="applicationName">applicationName.</param>
        /// <returns>AA List of <see cref="MeetingNotificationItemEntity"/>.</returns>
        Task<IList<MeetingNotificationItemEntity>> UploadMeetingInvite(IList<MeetingNotificationItemEntity> meetingNotificationItemEntities, string notificationType, string applicationName);

        /// <summary>
        /// Downloads email from blob.
        /// </summary>
        /// <param name="MeetinglNotificationItemEntities">List of <see cref="MeetingNotificationItemEntity"/>.</param>
        /// <param name="applicationName">applicationName.</param>
        /// <returns>A List of <see cref="MeetingNotificationItemEntity"/>.</returns>
        Task<IList<MeetingNotificationItemEntity>> DownloadMeetingInvite(IList<MeetingNotificationItemEntity> MeetinglNotificationItemEntities, string applicationName);
    }
}
