// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Extensions
{
    using System;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// EmailNotificationItemTableEntityExtension.
    /// </summary>
    public static class EmailNotificationItemTableEntityExtension
    {
        /// <summary>
        /// Converts <see cref="EmailNotificationItemTableEntity"/> to a <see cref="EmailNotificationItemEntity"/>.
        /// </summary>
        /// <param name="emailNotificationItemTableEntity"> EmailNotificationItemTableEntity. </param>
        /// <returns><see cref="EmailNotificationItemEntity"/>.</returns>
        public static EmailNotificationItemEntity ConvertToEmailNotificationItemEntity(this EmailNotificationItemTableEntity emailNotificationItemTableEntity)
        {
            if (emailNotificationItemTableEntity is null)
            {
                return null;
            }

            EmailNotificationItemEntity emailNotificationItemEntity = new EmailNotificationItemEntity();
            NotificationPriority notificationPriority = Enum.TryParse<NotificationPriority>(emailNotificationItemTableEntity.Priority, out notificationPriority) ? notificationPriority : NotificationPriority.Low;
            NotificationItemStatus notificationItemStatus = Enum.TryParse<NotificationItemStatus>(emailNotificationItemTableEntity.Status, out notificationItemStatus) ? notificationItemStatus : NotificationItemStatus.Queued;
            emailNotificationItemEntity.Priority = notificationPriority;
            emailNotificationItemEntity.Status = notificationItemStatus;
            emailNotificationItemEntity.PartitionKey = emailNotificationItemTableEntity.Application;
            emailNotificationItemEntity.RowKey = emailNotificationItemTableEntity.NotificationId;
            emailNotificationItemEntity.Application = emailNotificationItemTableEntity.Application;
            emailNotificationItemEntity.BCC = emailNotificationItemTableEntity.BCC;
            emailNotificationItemEntity.CC = emailNotificationItemTableEntity.CC;
            emailNotificationItemEntity.EmailAccountUsed = emailNotificationItemTableEntity.EmailAccountUsed;
            emailNotificationItemEntity.ErrorMessage = emailNotificationItemTableEntity.ErrorMessage;
            emailNotificationItemEntity.From = emailNotificationItemTableEntity.From;
            emailNotificationItemEntity.NotificationId = emailNotificationItemTableEntity.NotificationId;
            emailNotificationItemEntity.ReplyTo = emailNotificationItemTableEntity.ReplyTo;
            emailNotificationItemEntity.Sensitivity = emailNotificationItemTableEntity.Sensitivity;
            emailNotificationItemEntity.Subject = emailNotificationItemTableEntity.Subject;
            emailNotificationItemEntity.TemplateId = emailNotificationItemTableEntity.TemplateId;
            //emailNotificationItemEntity.TemplateData = emailNotificationItemTableEntity.TemplateData;
            emailNotificationItemEntity.Timestamp = emailNotificationItemTableEntity.Timestamp;
            emailNotificationItemEntity.To = emailNotificationItemTableEntity.To;
            emailNotificationItemEntity.TrackingId = emailNotificationItemTableEntity.TrackingId;
            emailNotificationItemEntity.TryCount = emailNotificationItemTableEntity.TryCount;
            emailNotificationItemEntity.ETag = emailNotificationItemTableEntity.ETag;
            emailNotificationItemEntity.SendOnUtcDate = emailNotificationItemTableEntity.SendOnUtcDate;
            return emailNotificationItemEntity;
        }
    }
}
