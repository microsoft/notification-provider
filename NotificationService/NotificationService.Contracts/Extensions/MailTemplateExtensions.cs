// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Extensions
{
    using System;
    using NotificationService.Common.Encryption;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// Extensions of the <see cref="MailTemplate"/> class.
    /// </summary>
    public static class MailTemplateExtensions
    {
        /// <summary>
        /// Converts <see cref="MailTemplate"/> to a <see cref="MailTemplateEntity"/>.
        /// </summary>
        /// <param name="mailTempalteItem">Email template item.</param>
        /// <param name="applicationName">Application associated to the notification item.</param>
        /// <param name="encryptionService">Instance of encryption service to protect the secure content before saving in datastore.</param>
        /// <returns><see cref="MailTemplateEntity"/>.</returns>
        public static MailTemplateEntity ToEntity(this MailTemplate mailTempalteItem, string applicationName, IEncryptionService encryptionService)
        {
            if (encryptionService is null)
            {
                throw new ArgumentNullException(nameof(encryptionService));
            }

            if (mailTempalteItem != null)
            {
                return new MailTemplateEntity()
                {
                    PartitionKey = applicationName,
                    RowKey = mailTempalteItem.TemplateId,
                    Application = applicationName,
                    TemplateId = mailTempalteItem.TemplateId,
                    Description = mailTempalteItem.Description,
                    TemplateType = mailTempalteItem.TemplateType,
                    Content = encryptionService.Encrypt(mailTempalteItem.Content),
                };
            }

            return null;
        }

        /// <summary>
        /// Converts <see cref="MailTemplateEntity"/> to a <see cref="MailTemplate"/>.
        /// </summary>
        /// <param name="mailTemplateEntity">Email template item.</param>
        /// <param name="encryptionService">Instance of encryption service to protect the secure content before saving in datastore.</param>
        /// <returns><see cref="MailTemplate"/>.</returns>
        public static MailTemplate ToContract(this MailTemplateEntity mailTemplateEntity, IEncryptionService encryptionService)
        {
            if (encryptionService is null)
            {
                throw new ArgumentNullException(nameof(encryptionService));
            }

            if (mailTemplateEntity != null)
            {
                return new MailTemplate
                {
                    TemplateId = mailTemplateEntity.TemplateId,
                    Description = mailTemplateEntity.Description,
                    TemplateType = mailTemplateEntity.TemplateType,
                    Content = encryptionService.Decrypt(mailTemplateEntity.Content),
                };
            }

            return null;
        }

        /// <summary>
        /// Converts <see cref="MailTemplateEntity"/> to a <see cref="MailTemplateInfo"/>.
        /// </summary>
        /// <param name="mailTemplateEntity">Email template item.</param>
        /// <returns><see cref="MailTemplateInfo"/>.</returns>
        public static MailTemplateInfo ToTemplateInfoContract(this MailTemplateEntity mailTemplateEntity)
        {
            if (mailTemplateEntity != null)
            {
                return new MailTemplateInfo
                {
                    TemplateId = mailTemplateEntity.TemplateId,
                    Description = mailTemplateEntity.Description,
                    TemplateType = mailTemplateEntity.TemplateType,
                };
            }

            return null;
        }
    }
}
