// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Extensions
{
    using System.Linq;

    /// <summary>
    /// Extensions of the <see cref="EmailNotificationItem"/> class.
    /// </summary>
    public static class EmailNotificationItemExtensions
    {
        /// <summary>
        /// Converts <see cref="EmailNotificationItem"/> to a <see cref="EmailNotificationItemEntity"/>.
        /// </summary>
        /// <param name="emailNotificationItem">Email Notification Item.</param>
        /// <param name="applicationName">Application associated to the notification item.</param>
        /// <returns><see cref="EmailNotificationItemEntity"/>.</returns>
        public static EmailNotificationItemEntity ToEntity(this EmailNotificationItem emailNotificationItem, string applicationName)
        {
            if (emailNotificationItem != null)
            {
                return new EmailNotificationItemEntity()
                {
                    Application = applicationName,
                    Attachments = emailNotificationItem?.Attachments?.Select(attachment => new NotificationAttachmentEntity { IsInline = attachment.IsInline, FileName = attachment?.FileName, FileBase64 = attachment?.FileBase64 }).ToList(),
                    BCC = emailNotificationItem?.BCC,
                    Body = emailNotificationItem?.Body,
                    CC = emailNotificationItem?.CC,
                    From = emailNotificationItem?.From,
                    Priority = emailNotificationItem.Priority,
                    ReplyTo = emailNotificationItem?.ReplyTo,
                    SendOnUtcDate = emailNotificationItem.SendOnUtcDate,
                    Sensitivity = emailNotificationItem.Sensitivity,
                    Subject = emailNotificationItem?.Subject,
                    To = emailNotificationItem?.To,
                    TemplateData = emailNotificationItem?.TemplateData,
                    TemplateId = emailNotificationItem?.TemplateId,
                    TrackingId = emailNotificationItem?.TrackingId,
                };
            }

            return null;
        }
    }
}
