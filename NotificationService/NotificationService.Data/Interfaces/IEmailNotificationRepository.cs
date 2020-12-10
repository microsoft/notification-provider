// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Data.Interfaces;

    /// <summary>
    /// Repository Interface for Email Notification Items.
    /// </summary>
    public interface IEmailNotificationRepository
    {
        /// <summary>
        /// Gets the email notification items from database for the input ids.
        /// </summary>
        /// <param name="notificationIds">List of notifications ids.</param>
        /// <returns>List of notitication items corresponding to input ids.</returns>
        Task<IList<EmailNotificationItemEntity>> GetEmailNotificationItemEntities(IList<string> notificationIds);

        /// <summary>
        /// Gets the email notification item from database for the input id.
        /// </summary>
        /// <param name="notificationId">A single notifications id.</param>
        /// <returns>notitication item corresponding to input id.</returns>
        Task<EmailNotificationItemEntity> GetEmailNotificationItemEntity(string notificationId);

        /// <summary>
        /// Creates entities in database for the input email notification items.
        /// </summary>
        /// <param name="emailNotificationItemEntities">List of <see cref="EmailNotificationItemEntity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task CreateEmailNotificationItemEntities(IList<EmailNotificationItemEntity> emailNotificationItemEntities);

        /// <summary>
        /// Saves the changes on email notification entities into database.
        /// </summary>
        /// <param name="emailNotificationItemEntities">List of <see cref="EmailNotificationItemEntity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateEmailNotificationItemEntities(IList<EmailNotificationItemEntity> emailNotificationItemEntities);

        /// <summary>
        /// Gets the list of Notification Items based on query expression.
        /// </summary>
        /// <typeparam name="T">of Type.</typeparam>
        /// <param name="notificationReportRequest">NotificationReportRequest param.</param>
        /// <returns>Returns list of Notification Responses.</returns>
        Task<Tuple<IList<EmailNotificationItemEntity>, Microsoft.Azure.Cosmos.Table.TableContinuationToken>> GetEmailNotifications(NotificationReportRequest notificationReportRequest);

        /// <summary>
        /// Gets the meeting notification items from database for the input ids.
        /// </summary>
        /// <param name="notificationIds">List of notifications ids.</param>
        /// <returns>List of notitication items corresponding to input ids.</returns>
        Task<IList<MeetingNotificationItemEntity>> GetMeetingNotificationItemEntities(IList<string> notificationIds);

        /// <summary>
        /// Gets the meeting notification item from database for the input id.
        /// </summary>
        /// <param name="notificationId">A single notifications id.</param>
        /// <returns>notitication item corresponding to input id.</returns>
        Task<MeetingNotificationItemEntity> GetMeetingNotificationItemEntity(string notificationId);

        /// <summary>
        /// Creates entities in database for the input meeting notification items.
        /// </summary>
        /// <param name="meetingNotificationItemEntity">List of <see cref="MeetingNotificationItemEntity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task CreateMeetingNotificationItemEntities(IList<MeetingNotificationItemEntity> meetingNotificationItemEntity);

        /// <summary>
        /// Saves the changes on meeting notification entities into database.
        /// </summary>
        /// <param name="meetingNotificationItemEntity">List of <see cref="MeetingNotificationItemEntity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateMeetingNotificationItemEntities(IList<MeetingNotificationItemEntity> meetingNotificationItemEntity);
    }
}
