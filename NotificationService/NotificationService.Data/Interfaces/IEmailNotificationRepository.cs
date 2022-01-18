// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models.Request;

    /// <summary>
    /// Repository Interface for Email Notification Items.
    /// </summary>
    public interface IEmailNotificationRepository
    {
        /// <summary>
        /// Gets the email notification items from database for the input ids.
        /// </summary>
        /// <param name="notificationIds">List of notifications ids.</param>
        /// <param name="applicationName">The Application Name (Optional).</param>
        /// <returns>List of notitication items corresponding to input ids.</returns>
        Task<IList<EmailNotificationItemEntity>> GetEmailNotificationItemEntities(IList<string> notificationIds, string applicationName = null);

        /// <summary>
        /// Gets the email notification item from database for the input id.
        /// </summary>
        /// <param name="notificationId">A single notifications id.</param>
        /// <param name="applicationName">The Application Name (Optional).</param>
        /// <returns>notitication item corresponding to input id.</returns>
        Task<EmailNotificationItemEntity> GetEmailNotificationItemEntity(string notificationId, string applicationName = null);

        /// <summary>
        /// Creates entities in database for the input email notification items.
        /// </summary>
        /// <param name="emailNotificationItemEntities">List of <see cref="EmailNotificationItemEntity"/>.</param>
        /// <param name="applicationName">The applicationName (Optional).</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task CreateEmailNotificationItemEntities(IList<EmailNotificationItemEntity> emailNotificationItemEntities, string applicationName = null);

        /// <summary>
        /// Saves the changes on email notification entities into database.
        /// </summary>
        /// <param name="emailNotificationItemEntities">List of <see cref="EmailNotificationItemEntity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateEmailNotificationItemEntities(IList<EmailNotificationItemEntity> emailNotificationItemEntities);

        /// <summary>
        /// Gets the list of Notification Items based on query expression.
        /// </summary>
        /// <param name="notificationReportRequest">NotificationReportRequest param.</param>
        /// <returns>Returns list of Notification Responses.</returns>
        Task<Tuple<IList<EmailNotificationItemEntity>, Microsoft.Azure.Cosmos.Table.TableContinuationToken>> GetEmailNotifications(NotificationReportRequest notificationReportRequest);

        /// <summary>
        /// Gets the meeting notification items from database for the input ids.
        /// </summary>
        /// <param name="notificationIds">List of notifications ids.</param>
        /// <param name="applicationName"> applicationName as containerName. </param>
        /// <returns>List of notitication items corresponding to input ids.</returns>
        Task<IList<MeetingNotificationItemEntity>> GetMeetingNotificationItemEntities(IList<string> notificationIds, string applicationName);

        /// <summary>
        /// Gets the meeting notification item from database for the input id.
        /// </summary>
        /// <param name="notificationId">A single notifications id.</param>
        /// <param name="applicationName">applicationName.</param>
        /// <returns>notitication item corresponding to input id.</returns>
        Task<MeetingNotificationItemEntity> GetMeetingNotificationItemEntity(string notificationId, string applicationName);

        /// <summary>
        /// Creates entities in database for the input meeting notification items.
        /// </summary>
        /// <param name="meetingNotificationItemEntity">List of <see cref="MeetingNotificationItemEntity"/>.</param>
        /// <param name="applicationName"> application name as container.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task CreateMeetingNotificationItemEntities(IList<MeetingNotificationItemEntity> meetingNotificationItemEntity, string applicationName);

        /// <summary>
        /// Saves the changes on meeting notification entities into database.
        /// </summary>
        /// <param name="meetingNotificationItemEntity">List of <see cref="MeetingNotificationItemEntity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateMeetingNotificationItemEntities(IList<MeetingNotificationItemEntity> meetingNotificationItemEntity);

        /// <summary>
        /// Gets the list of Meeting Invite Notification Items based on query expression.
        /// </summary>
        /// <param name="meetingInviteReportRequest">NotificationReportRequest param.</param>
        /// <returns>Returns list of Notification Responses.</returns>
        Task<Tuple<IList<MeetingNotificationItemEntity>, Microsoft.Azure.Cosmos.Table.TableContinuationToken>> GetMeetingInviteNotifications(NotificationReportRequest meetingInviteReportRequest);

        /// <summary>
        /// Get EMailNotification Entities for given data range.
        /// Maximum range allowed is 10 hours. It will return data for 10 hours at a time.
        /// </summary>
        /// <param name="dateRange"> input param for dateRange.</param>
        /// <param name="applicationName">application name.</param>
        /// <param name="statusList">List of Status for which notificaiton items need to be fetched.</param>
        /// <param name="loadBody"> by default it is false. if false it will not populate body, attachments etc. </param>
        /// <returns>A <see cref="Task"/> represents the return of the asynchronous operation.</returns>
        Task<IList<EmailNotificationItemEntity>> GetPendingOrFailedEmailNotificationsByDateRange(DateTimeRange dateRange, string applicationName, List<NotificationItemStatus> statusList, bool loadBody = false);

        /// <summary>
        /// Get MeetingNotification Entities for given data range.
        /// Maximum range allowed is 10 hours. It will return data for 10 hours at a time.
        /// </summary>
        /// <param name="dateRange"> input param for dateRange.</param>
        /// <param name="applicationName">application name.</param>
        /// <param name="statusList">List of Status for which notificaiton items need to be fetched.</param>
        /// <param name="loadBody"> by default it is false. if false it will not populate body, attachments etc. </param>
        /// <returns>A <see cref="Task"/> represents the return of the asynchronous operation.</returns>
        Task<IList<MeetingNotificationItemEntity>> GetPendingOrFailedMeetingNotificationsByDateRange(DateTimeRange dateRange, string applicationName, List<NotificationItemStatus> statusList, bool loadBody = false);

    }
}
