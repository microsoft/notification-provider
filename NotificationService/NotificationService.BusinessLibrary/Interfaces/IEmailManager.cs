// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models;
    using NotificationService.Contracts.Models.Request;

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
        /// <returns>A List of <see cref="NotificationResponse"/>.</returns>
        IList<NotificationResponse> NotificationEntitiesToResponse(IList<NotificationResponse> notificationResponses, IList<MeetingNotificationItemEntity> notificationItemEntities);

        /// <summary>
        /// Gets the notification message body asynchronous.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="notification">The notification.</param>
        /// <returns>A <see cref="MessageBody"/>.</returns>
        Task<MessageBody> GetMeetingInviteBodyAsync(string applicationName, MeetingNotificationItemEntity notification);

        /// <summary>
        /// Get EmailNotificaiton object from storage/database.
        /// </summary>
        /// <param name="applicationName">applicatoin Name for the notification.</param>
        /// <param name="dateRange">daterange for notification search.</param>
        /// <param name="statusList">Status List of Notification items.</param>
        /// <returns>A <see cref="EmailNotificationItemTableEntity"/>.</returns>
        Task<IList<EmailNotificationItemEntity>> GetEmailNotificationsByDateRangeAndStatus(string applicationName, DateTimeRange dateRange, List<NotificationItemStatus> statusList);
    }
}
