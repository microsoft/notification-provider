// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using NotificationService.Common.Configurations;

    /// <summary>
    /// Extensions of the <see cref="EmailNotificationItemEntity"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class EmailNotificationItemEntityExtensions
    {
        /// <summary>
        /// Converts <see cref="EmailNotificationItemEntity"/> to a <see cref="EmailMessage"/>.
        /// </summary>
        /// <param name="emailNotificationItemEntity">Email Notification Item Entity.</param>
        /// <param name="body">Message Bosy.</param>
        /// <returns><see cref="EmailMessage"/>.</returns>
        public static EmailMessage ToGraphEmailMessage(this EmailNotificationItemEntity emailNotificationItemEntity, MessageBody body)
        {
            return new EmailMessage()
            {
                Subject = emailNotificationItemEntity?.Subject,
                Body = body,
                ToRecipients = emailNotificationItemEntity.To.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(torecipient => new Recipient { EmailAddress = new EmailAddress { Address = torecipient } }).ToList(),
                CCRecipients = emailNotificationItemEntity.CC?.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(ccrecipient => new Recipient { EmailAddress = new EmailAddress { Address = ccrecipient } }).ToList(),
                BCCRecipients = emailNotificationItemEntity.BCC?.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(bccrecipient => new Recipient { EmailAddress = new EmailAddress { Address = bccrecipient } }).ToList(),
                ReplyToRecipients = emailNotificationItemEntity.ReplyTo?.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(reploytorecipient => new Recipient { EmailAddress = new EmailAddress { Address = reploytorecipient } }).ToList(),
                Attachments = emailNotificationItemEntity.Attachments?.Select(attachment => new FileAttachment
                { Name = attachment.FileName, ContentBytes = attachment.FileBase64, IsInline = attachment.IsInline }).ToList(),
                FromAccount = !string.IsNullOrWhiteSpace(emailNotificationItemEntity.From) ? new Recipient() { EmailAddress = new EmailAddress() { Address = emailNotificationItemEntity.From } } : null,
                SingleValueExtendedProperties = new List<SingleValueExtendedProperty> { new SingleValueExtendedProperty { Id = "SystemTime 0x3FEF", Value = emailNotificationItemEntity.SendOnUtcDate.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture) } },
                Importance = (Models.Graph.ImportanceType)Enum.Parse(typeof(Contracts.Models.Graph.ImportanceType), emailNotificationItemEntity.Priority.ToString()),
            };
        }

        /// <summary>
        /// Converts <see cref="EmailNotificationItemEntity"/> to a <see cref="NotificationReportResponse"/>.
        /// </summary>
        /// <param name="emailNotificationItemEntity">Email Notification Item Entity.</param>
        /// <returns><see cref="NotificationReportResponse"/>.</returns>
        public static NotificationReportResponse ToNotificationReportResponse(EmailNotificationItemEntity emailNotificationItemEntity) => new NotificationReportResponse()
        {
            NotificationId = emailNotificationItemEntity?.NotificationId,
            Application = emailNotificationItemEntity?.Application,
            EmailAccountUsed = emailNotificationItemEntity?.EmailAccountUsed,
            NotifyType = emailNotificationItemEntity?.NotifyType.ToString(),
            Status = emailNotificationItemEntity?.Status.ToString(),
            Priority = emailNotificationItemEntity?.Priority.ToString(),
            Sensitivity = emailNotificationItemEntity?.Sensitivity.ToString(),
            ErrorMessage = emailNotificationItemEntity?.ErrorMessage,
            TryCount = emailNotificationItemEntity?.TryCount ?? 0,
            SendOnUtcDate = emailNotificationItemEntity?.SendOnUtcDate ?? DateTime.MinValue,
            CreatedDateTime = emailNotificationItemEntity?.CreatedDateTime ?? DateTime.MinValue,
            UpdatedDateTime = emailNotificationItemEntity?.UpdatedDateTime ?? DateTime.MinValue,
            To = emailNotificationItemEntity?.To,
            From = emailNotificationItemEntity?.From,
            CC = emailNotificationItemEntity?.CC,
            BCC = emailNotificationItemEntity?.BCC,
            ReplyTo = emailNotificationItemEntity?.ReplyTo,
            Subject = emailNotificationItemEntity?.Subject,
            TrackingId = emailNotificationItemEntity?.TrackingId,
        };

        /// <summary>
        /// Converts to directsendemailmessage.
        /// </summary>
        /// <param name="emailNotificationItemEntity">The email notification item entity.</param>
        /// <param name="body">The body.</param>
        /// <param name="directSendSetting">The direct send setting.</param>
        /// <returns>A <see cref="EmailMessage"/>.</returns>
        public static DirectSend.Models.Mail.EmailMessage ToDirectSendEmailMessage(this EmailNotificationItemEntity emailNotificationItemEntity, MessageBody body, DirectSendSetting directSendSetting)
        {
            return new DirectSend.Models.Mail.EmailMessage()
            {
                Subject = emailNotificationItemEntity?.Subject,
                Content = body?.Content,
                ToAddresses = emailNotificationItemEntity.To?.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                                 .Select(torecipient => new DirectSend.Models.Mail.EmailAddress { Address = torecipient }).ToList(),
                CcAddresses = emailNotificationItemEntity.CC?.Split(Common.ApplicationConstants.SplitCharacter, System.StringSplitOptions.RemoveEmptyEntries)
                                 .Select(ccrecipient => new DirectSend.Models.Mail.EmailAddress { Address = ccrecipient }).ToList(),
                FromAddresses = new List<DirectSend.Models.Mail.EmailAddress> { new DirectSend.Models.Mail.EmailAddress { Name = directSendSetting?.FromAddressDisplayName, Address = directSendSetting?.FromAddress } },
                FileName = emailNotificationItemEntity.Attachments?.Select(attachment => attachment.FileName).ToList(),
                FileContent = emailNotificationItemEntity.Attachments?.Select(attachment => attachment.FileBase64).ToList(),
                Importance = (DirectSend.Models.Mail.EmailMessage.ImportanceType)Enum.Parse(typeof(DirectSend.Models.Mail.EmailMessage.ImportanceType), emailNotificationItemEntity.Priority.ToString()),
            };
        }
    }
}
