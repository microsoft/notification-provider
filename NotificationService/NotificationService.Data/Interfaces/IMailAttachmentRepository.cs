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
        /// <param name="applicationName">applicationName.</param>
        /// <returns>AA List of <see cref="MeetingNotificationItemEntity"/>.</returns>
        Task<IList<MeetingNotificationItemEntity>> UploadMeetingInvite(IList<MeetingNotificationItemEntity> meetingNotificationItemEntities, string applicationName);

        /// <summary>
        /// Downloads email from blob.
        /// </summary>
        /// <param name="meetinglNotificationItemEntities">List of <see cref="MeetingNotificationItemEntity"/>.</param>
        /// <param name="applicationName">applicationName.</param>
        /// <returns>A List of <see cref="MeetingNotificationItemEntity"/>.</returns>
        Task<IList<MeetingNotificationItemEntity>> DownloadMeetingInvite(IList<MeetingNotificationItemEntity> meetinglNotificationItemEntities, string applicationName);
    }
}
