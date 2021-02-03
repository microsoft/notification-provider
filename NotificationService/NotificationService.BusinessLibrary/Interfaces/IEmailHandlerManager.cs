// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models;

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
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<IList<NotificationResponse>> ResendEmailNotifications(string applicationName, string[] notificationIds);

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
    }
}
