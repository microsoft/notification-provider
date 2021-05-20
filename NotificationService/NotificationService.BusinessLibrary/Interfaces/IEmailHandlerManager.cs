// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models;
    using NotificationService.Contracts.Models.Request;

    /// <summary>
    /// Interface for Notification Handler Manager.
    /// </summary>
    public interface IEmailHandlerManager
    {
        /// <summary>
        /// Requeue email notification items to be resent.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="notificationIds">Array of notification Ids to be resent.</param>
        /// <param name="notifType">Type of notification (Mail, Meeting).</param>
        /// <param name="ignoreAlreadySent"> ignore alredysent items.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<IList<NotificationResponse>> ResendNotifications(string applicationName, string[] notificationIds, NotificationType notifType = NotificationType.Mail, bool ignoreAlreadySent = false);

        /// <summary>
        /// Queue email notification items.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="emailNotificationItems">Array of email notification items.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<IList<NotificationResponse>> QueueEmailNotifications(string applicationName, EmailNotificationItem[] emailNotificationItems);

        /// <summary>
        /// Queue email notification items.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="meetingNotificationItems">Array of email notification items.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<IList<NotificationResponse>> QueueMeetingNotifications(string applicationName, MeetingNotificationItem[] meetingNotificationItems);

        /// <summary>
        /// Requeue email notification items to be resent.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="dateRange"> daterange object containing start and end dates.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<IList<NotificationResponse>> ResendEmailNotificationsByDateRange(string applicationName, DateTimeRange dateRange);

        /// <summary>
        /// Requeue meeting notification items to be resent basing on date range.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="dateRange"> daterange object containing start and end dates.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<IList<NotificationResponse>> ResendMeetingNotificationsByDateRange(string applicationName, DateTimeRange dateRange);
    }
}
