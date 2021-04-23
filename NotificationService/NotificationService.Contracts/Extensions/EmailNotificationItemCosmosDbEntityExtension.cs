// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Extensions
{
    using System;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// EmailNotificationItemCosmosDbEntityExtension.
    /// </summary>
    public static class EmailNotificationItemCosmosDbEntityExtension
    {
        /// <summary>
        /// Converts <see cref="EmailNotificationItemCosmosDbEntity"/> to a <see cref="EmailNotificationItemEntity"/>.
        /// </summary>
        /// <param name="emailNotificationItemDbEntity"> EmailNotificationItemCosmosDbEntity. </param>
        /// <returns><see cref="EmailNotificationItemEntity"/>.</returns>
        public static EmailNotificationItemEntity ConvertToEmailNotificationItemEntity(this EmailNotificationItemCosmosDbEntity emailNotificationItemDbEntity)
        {
            if (emailNotificationItemDbEntity is null)
            {
                return null;
            }

            EmailNotificationItemEntity emailNotificationItemEntity = new EmailNotificationItemEntity();
            NotificationPriority notificationPriority = Enum.TryParse<NotificationPriority>(emailNotificationItemDbEntity.Priority, out notificationPriority) ? notificationPriority : NotificationPriority.Low;
            NotificationItemStatus notificationItemStatus = Enum.TryParse<NotificationItemStatus>(emailNotificationItemDbEntity.Status, out notificationItemStatus) ? notificationItemStatus : NotificationItemStatus.Queued;
            emailNotificationItemEntity.Priority = notificationPriority;
            emailNotificationItemEntity.Status = notificationItemStatus;
            emailNotificationItemEntity.PartitionKey = emailNotificationItemDbEntity.Application;
            emailNotificationItemEntity.RowKey = emailNotificationItemDbEntity.NotificationId;
            emailNotificationItemEntity.Application = emailNotificationItemDbEntity.Application;
            emailNotificationItemEntity.BCC = emailNotificationItemDbEntity.BCC;
            emailNotificationItemEntity.CC = emailNotificationItemDbEntity.CC;
            emailNotificationItemEntity.EmailAccountUsed = emailNotificationItemDbEntity.EmailAccountUsed;
            emailNotificationItemEntity.ErrorMessage = emailNotificationItemDbEntity.ErrorMessage;
            emailNotificationItemEntity.From = emailNotificationItemDbEntity.From;
            emailNotificationItemEntity.NotificationId = emailNotificationItemDbEntity.NotificationId;
            emailNotificationItemEntity.ReplyTo = emailNotificationItemDbEntity.ReplyTo;
            emailNotificationItemEntity.Sensitivity = emailNotificationItemDbEntity.Sensitivity;
            emailNotificationItemEntity.Subject = emailNotificationItemDbEntity.Subject;
            emailNotificationItemEntity.TemplateId = emailNotificationItemDbEntity.TemplateId;
            //emailNotificationItemEntity.TemplateData = emailNotificationItemDbEntity.TemplateData;
            emailNotificationItemEntity.Timestamp = emailNotificationItemDbEntity.Timestamp;
            emailNotificationItemEntity.To = emailNotificationItemDbEntity.To;
            emailNotificationItemEntity.TrackingId = emailNotificationItemDbEntity.TrackingId;
            emailNotificationItemEntity.TryCount = emailNotificationItemDbEntity.TryCount;
            emailNotificationItemEntity.ETag = emailNotificationItemDbEntity.ETag;
            emailNotificationItemEntity.SendOnUtcDate = emailNotificationItemDbEntity.SendOnUtcDate;
            emailNotificationItemEntity.Id = emailNotificationItemDbEntity.Id;
            return emailNotificationItemEntity;
        }
    }
}
