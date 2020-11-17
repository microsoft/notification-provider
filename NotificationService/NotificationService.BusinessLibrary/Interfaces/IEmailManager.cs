// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts;

    /// <summary>
    /// Interface for Email Manager Common functions.
    /// </summary>
    public interface IEmailManager
    {
        /// <summary>
        /// Get Notification Message Body Async.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="notification">email notification item entity.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<MessageBody> GetNotificationMessageBodyAsync(string applicationName, EmailNotificationItemEntity notification);

        /// <summary>s
        /// Creates the notification entity records in database with the input status.
        /// </summary>
        /// <param name="applicationName">Application Name.</param>
        /// <param name="emailNotificationItems">Email Notification Items.</param>
        /// <param name="status">Status.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IList<EmailNotificationItemEntity>> CreateNotificationEntities(string applicationName, EmailNotificationItem[] emailNotificationItems, NotificationItemStatus status);

        IList<NotificationResponse> NotificationEntitiesToResponse(IList<NotificationResponse> notificationResponses, IList<EmailNotificationItemEntity> notificationItemEntities);
    }
}
