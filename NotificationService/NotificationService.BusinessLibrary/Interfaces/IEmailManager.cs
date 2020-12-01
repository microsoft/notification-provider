// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models;

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

        /// <summary>s
        /// Creates the notification entity records in database with the input status.
        /// </summary>
        /// <param name="applicationName">Application Name.</param>
        /// <param name="meetingNotificationItems">Email Notification Items.</param>
        /// <param name="status">Status.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IList<MeetingNotificationItemEntity>> CreateMeetingNotificationEntities(string applicationName, MeetingNotificationItem[] meetingNotificationItems, NotificationItemStatus status);

        /// <summary>
        /// Notifications the entities to response.
        /// </summary>
        /// <param name="notificationResponses">The notification responses.</param>
        /// <param name="notificationItemEntities">The notification item entities.</param>
        /// <returns>A <see cref="NotificationResponse"></see>/>.</returns>
        IList<NotificationResponse> NotificationEntitiesToResponse(IList<NotificationResponse> notificationResponses, IList<EmailNotificationItemEntity> notificationItemEntities);

        /// <summary>
        /// Notifications the entities to response.
        /// </summary>
        /// <param name="notificationResponses">The notification responses.</param>
        /// <param name="notificationItemEntities">The notification item entities.</param>
        /// <returns></returns>
        IList<NotificationResponse> NotificationEntitiesToResponse(IList<NotificationResponse> notificationResponses, IList<MeetingNotificationItemEntity> notificationItemEntities);
    }
}
