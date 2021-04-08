// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data.Repositories
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using NotificationService.Common;
    using NotificationService.Common.Encryption;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// The Mail Attachment Repositoy class.
    /// </summary>
    public class MailAttachmentRepository : IMailAttachmentRepository
    {
        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of <see cref="ICloudStorageClient"/>.
        /// </summary>
        private readonly ICloudStorageClient cloudStorageClient;

        /// <summary>
        /// Instance of <see cref="IEncryptionService"/>.
        /// </summary>
        private readonly IEncryptionService encryptionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MailAttachmentRepository"/> class.
        /// </summary>
        /// <param name="logger">The Logger instance.</param>
        /// <param name="cloudStorageClient">The Cloud Storage Client instance.</param>
        /// <param name="encryptionService">The IEncryptionService instance.</param>
        public MailAttachmentRepository(ILogger logger, ICloudStorageClient cloudStorageClient, IEncryptionService encryptionService)
        {
            this.logger = logger;
            this.cloudStorageClient = cloudStorageClient;
            this.encryptionService = encryptionService;
        }

        /// <inheritdoc/>
        public async Task<IList<EmailNotificationItemEntity>> UploadEmail(IList<EmailNotificationItemEntity> emailNotificationItemEntities, string notificationType, string applicationName)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.NotificationType] = notificationType;
            traceProps[AIConstants.EmailNotificationCount] = emailNotificationItemEntities?.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.UploadEmail)} method of {nameof(MailAttachmentRepository)}.", traceProps);
            IList<EmailNotificationItemEntity> notificationEntities = new List<EmailNotificationItemEntity>();
            if (!(emailNotificationItemEntities is null) && emailNotificationItemEntities.Count > 0)
            {
                foreach (var item in emailNotificationItemEntities)
                {
                    var notificationEntity = item;
                    var blobEmailData = new BlobEmailData
                    {
                        NotificationId = item.NotificationId,
                        Body = item.Body,
                        Attachments = item.Attachments,
                        TemplateData = item.TemplateData,
                    };
                    var blobpath = this.GetBlobPath(applicationName, item.NotificationId, ApplicationConstants.EmailNotificationsFolderName);
                    var uloadedblobpath = await this.cloudStorageClient.UploadBlobAsync(blobpath, this.encryptionService.Encrypt(JsonConvert.SerializeObject(blobEmailData))).ConfigureAwait(false);
                    notificationEntities.Add(notificationEntity);
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.UploadEmail)} method of {nameof(MailAttachmentRepository)}.", traceProps);
            return notificationEntities;
        }

        /// <inheritdoc/>
        public async Task<IList<MeetingNotificationItemEntity>> UploadMeetingInvite(IList<MeetingNotificationItemEntity> meetingNotificationItemEntities, string notificationType, string applicationName)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.NotificationType] = notificationType;
            traceProps[AIConstants.MeetingNotificationCount] = meetingNotificationItemEntities?.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.UploadMeetingInvite)} method of {nameof(MailAttachmentRepository)}.", traceProps);
            IList<MeetingNotificationItemEntity> notificationEntities = new List<MeetingNotificationItemEntity>();
            if (!(meetingNotificationItemEntities is null) && meetingNotificationItemEntities.Count > 0)
            {
                foreach (var item in meetingNotificationItemEntities)
                {
                    var notificationEntity = item;
                    var blobEmailData = new BlobEmailData
                    {
                        NotificationId = item.NotificationId,
                        Body = item.Body,
                        Attachments = item.Attachments,
                        TemplateData = item.TemplateData,
                    };
                    var blobpath = this.GetBlobPath(applicationName, item.NotificationId, ApplicationConstants.MeetingNotificationsFolderName);
                    var uloadedblobpath = await this.cloudStorageClient.UploadBlobAsync(blobpath, this.encryptionService.Encrypt(JsonConvert.SerializeObject(blobEmailData))).ConfigureAwait(false);
                    notificationEntities.Add(item);
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.UploadEmail)} method of {nameof(MailAttachmentRepository)}.", traceProps);
            return notificationEntities;
        }

        /// <inheritdoc/>
        public async Task<IList<EmailNotificationItemEntity>> DownloadEmail(IList<EmailNotificationItemEntity> emailNotificationItemEntities, string applicationName)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.EmailNotificationCount] = emailNotificationItemEntities?.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.DownloadEmail)} method of {nameof(MailAttachmentRepository)}.", traceProps);
            IList<EmailNotificationItemEntity> notificationEntities = new List<EmailNotificationItemEntity>();
            if (!(emailNotificationItemEntities is null) && emailNotificationItemEntities.Count > 0)
            {
                foreach (var item in emailNotificationItemEntities)
                {
                    EmailNotificationItemEntity notificationEntity = item;
                    var blobPath = this.GetBlobPath(applicationName, item.NotificationId, ApplicationConstants.EmailNotificationsFolderName);
                    var encryptedData = await this.cloudStorageClient.DownloadBlobAsync(blobPath).ConfigureAwait(false);
                    var decryptedData = this.encryptionService.Decrypt(encryptedData);
                    var blobEmailData = JsonConvert.DeserializeObject<BlobEmailData>(decryptedData);
                    notificationEntity.Attachments = blobEmailData.Attachments;
                    notificationEntity.Body = blobEmailData.Body;
                    notificationEntity.TemplateData = blobEmailData.TemplateData;
                    notificationEntities.Add(notificationEntity);
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.DownloadEmail)} method of {nameof(MailAttachmentRepository)}.", traceProps);
            return notificationEntities;
        }

        /// <inheritdoc/>
        public async Task<IList<MeetingNotificationItemEntity>> DownloadMeetingInvite(IList<MeetingNotificationItemEntity> meetingNotificationItemEntities, string applicationName)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.MeetingNotificationCount] = meetingNotificationItemEntities?.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.DownloadMeetingInvite)} method of {nameof(MailAttachmentRepository)}.", traceProps);
            IList<MeetingNotificationItemEntity> notificationEntities = new List<MeetingNotificationItemEntity>();
            if (!(meetingNotificationItemEntities is null) && meetingNotificationItemEntities.Count > 0)
            {
                foreach (var item in meetingNotificationItemEntities)
                {
                    MeetingNotificationItemEntity notificationEntity = item;
                    var blobPath = this.GetBlobPath(applicationName, item.NotificationId, ApplicationConstants.MeetingNotificationsFolderName);
                    var encryptedData = await this.cloudStorageClient.DownloadBlobAsync(blobPath).ConfigureAwait(false);
                    var decryptedData = this.encryptionService.Decrypt(encryptedData);
                    var blobEmailData = JsonConvert.DeserializeObject<BlobEmailData>(decryptedData);
                    notificationEntity.Attachments = blobEmailData.Attachments;
                    notificationEntity.Body = blobEmailData.Body;
                    notificationEntity.TemplateData = blobEmailData.TemplateData;
                    notificationEntities.Add(notificationEntity);
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.DownloadMeetingInvite)} method of {nameof(MailAttachmentRepository)}.", traceProps);
            return notificationEntities;
        }

        private string GetBlobPath(string applicationName, string notificationId, string folderName)
        {
            return $"{applicationName}/{folderName}/{notificationId}";
        }
    }
}
