// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
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
        /// Initializes a new instance of the <see cref="MailAttachmentRepository"/> class.
        /// </summary>
        /// <param name="logger">The Logger instance.</param>
        /// <param name="cloudStorageClient">The Cloud Storage Client instance.</param>
        public MailAttachmentRepository(ILogger logger, ICloudStorageClient cloudStorageClient)
        {
            this.logger = logger;
            this.cloudStorageClient = cloudStorageClient;
        }

        /// <inheritdoc/>
        public async Task<IList<EmailNotificationItemEntity>> UploadAttachment(IList<EmailNotificationItemEntity> emailNotificationItemEntities, string notificationType, string applicationName)
        {
            this.logger.TraceInformation($"Started {nameof(this.UploadAttachment)} method of {nameof(MailAttachmentRepository)}.");
            IList<EmailNotificationItemEntity> notificationEntities = new List<EmailNotificationItemEntity>();
            if (!(emailNotificationItemEntities is null) && emailNotificationItemEntities.Count > 0)
            {
                foreach (var item in emailNotificationItemEntities)
                {
                    EmailNotificationItemEntity notificationEntity = item;
                    if (item.Attachments.Any())
                    {
                        List<NotificationAttachmentReference> attachmentReferences = new List<NotificationAttachmentReference>();
                        string blobReference = string.Empty;
                        foreach (var attachment in item.Attachments)
                        {
                            string blobPath = string.Concat(notificationType, "/", item.NotificationId, "/", attachment.FileName);
                            blobReference = await this.cloudStorageClient.UploadAttachmentToBlobAsync(applicationName, blobPath, attachment.FileBase64).ConfigureAwait(false);
                            attachmentReferences.Add(new NotificationAttachmentReference
                            {
                                FileName = attachment.FileName,
                                FileReference = blobReference,
                                IsInline = attachment.IsInline,
                            });
                        }

                        notificationEntity.AttachmentReference = JsonConvert.SerializeObject(attachmentReferences);
                    }

                    notificationEntities.Add(notificationEntity);
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.UploadAttachment)} method of {nameof(MailAttachmentRepository)}.");
            return notificationEntities;
        }

        /// <inheritdoc/>
        public async Task<IList<MeetingNotificationItemEntity>> UploadMeetingAttachments(IList<MeetingNotificationItemEntity> emailNotificationItemEntities, string notificationType, string applicationName)
        {
            this.logger.TraceInformation($"Started {nameof(this.UploadMeetingAttachments)} method of {nameof(MailAttachmentRepository)}.");
            IList<MeetingNotificationItemEntity> notificationEntities = new List<MeetingNotificationItemEntity>();
            if (!(emailNotificationItemEntities is null) && emailNotificationItemEntities.Count > 0)
            {
                foreach (var item in emailNotificationItemEntities)
                {
                    MeetingNotificationItemEntity notificationEntity = item;
                    if (item.Attachments.Any())
                    {
                        List<NotificationAttachmentReference> attachmentReferences = new List<NotificationAttachmentReference>();
                        string blobReference = string.Empty;
                        foreach (var attachment in item.Attachments)
                        {
                            string blobPath = string.Concat(notificationType, "/", item.NotificationId, "/", attachment.FileName);
                            blobReference = await this.cloudStorageClient.UploadAttachmentToBlobAsync(applicationName, blobPath, attachment.FileBase64).ConfigureAwait(false);
                            attachmentReferences.Add(new NotificationAttachmentReference
                            {
                                FileName = attachment.FileName,
                                FileReference = blobReference,
                                IsInline = attachment.IsInline,
                            });
                        }

                        notificationEntity.AttachmentReference = JsonConvert.SerializeObject(attachmentReferences);
                    }

                    notificationEntities.Add(notificationEntity);
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.UploadMeetingAttachments)} method of {nameof(MailAttachmentRepository)}.");
            return notificationEntities;
        }


        /// <inheritdoc/>
        public async Task<IList<EmailNotificationItemEntity>> DownloadAttachment(IList<EmailNotificationItemEntity> emailNotificationItemEntities, string applicationName)
        {
            this.logger.TraceInformation($"Started {nameof(this.DownloadAttachment)} method of {nameof(MailAttachmentRepository)}.");
            IList<EmailNotificationItemEntity> notificationEntities = new List<EmailNotificationItemEntity>();
            if (!(emailNotificationItemEntities is null) && emailNotificationItemEntities.Count > 0)
            {
                foreach (var item in emailNotificationItemEntities)
                {
                    EmailNotificationItemEntity notificationEntity = item;
                    if (!string.IsNullOrEmpty(item.AttachmentReference))
                    {
                        List<NotificationAttachmentReference> attachmentReferences = JsonConvert.DeserializeObject<List<NotificationAttachmentReference>>(item.AttachmentReference);
                        IList<NotificationAttachmentEntity> attachments = new List<NotificationAttachmentEntity>();
                        foreach (var reference in attachmentReferences)
                        {
                            string content = await this.cloudStorageClient.DownloadAttachmentFromBlobAsync(applicationName, reference.FileReference).ConfigureAwait(false);
                            NotificationAttachmentEntity attachment = new NotificationAttachmentEntity()
                            {
                                FileBase64 = content,
                                FileName = reference.FileName,
                                IsInline = reference.IsInline,
                            };
                            attachments.Add(attachment);
                        }

                        notificationEntity.Attachments = attachments;
                    }

                    notificationEntities.Add(notificationEntity);
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.DownloadAttachment)} method of {nameof(MailAttachmentRepository)}.");
            return notificationEntities;
        }

        /// <inheritdoc/>
        public async Task<IList<MeetingNotificationItemEntity>> DownloadMeetingAttachment(IList<MeetingNotificationItemEntity> emailNotificationItemEntities, string applicationName)
        {
            this.logger.TraceInformation($"Started {nameof(this.DownloadMeetingAttachment)} method of {nameof(MailAttachmentRepository)}.");
            IList<MeetingNotificationItemEntity> notificationEntities = new List<MeetingNotificationItemEntity>();
            if (!(emailNotificationItemEntities is null) && emailNotificationItemEntities.Count > 0)
            {
                foreach (var item in emailNotificationItemEntities)
                {
                    MeetingNotificationItemEntity notificationEntity = item;
                    if (!string.IsNullOrEmpty(item.AttachmentReference))
                    {
                        List<NotificationAttachmentReference> attachmentReferences = JsonConvert.DeserializeObject<List<NotificationAttachmentReference>>(item.AttachmentReference);
                        IList<NotificationAttachmentEntity> attachments = new List<NotificationAttachmentEntity>();
                        foreach (var reference in attachmentReferences)
                        {
                            string content = await this.cloudStorageClient.DownloadAttachmentFromBlobAsync(applicationName, reference.FileReference).ConfigureAwait(false);
                            NotificationAttachmentEntity attachment = new NotificationAttachmentEntity()
                            {
                                FileBase64 = content,
                                FileName = reference.FileName,
                                IsInline = reference.IsInline,
                            };
                            attachments.Add(attachment);
                        }

                        notificationEntity.Attachments = attachments;
                    }

                    notificationEntities.Add(notificationEntity);
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.DownloadMeetingAttachment)} method of {nameof(MailAttachmentRepository)}.");
            return notificationEntities;
        }
    }
}
