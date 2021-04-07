// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using NotificationService.Contracts;

    /// <summary>
    /// Interface for Notification Report Manager.
    /// </summary>
    public interface INotificationReportManager
    {
        /// <summary>
        /// Gets Notification Details for Reporting Requirements.
        /// </summary>
        /// <param name="notificationReportRequest">Request to filter Notifications.</param>
        /// <returns>List of Notifications based on filters passed.</returns>
        Task<Tuple<IList<NotificationReportResponse>, TableContinuationToken>> GetReportNotifications(NotificationReportRequest notificationReportRequest);

        /// <summary>
        /// Gets the Notification Message corresponding to the NotificationId.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="notificationId">notificationId.</param>
        /// <returns>A <see cref="Task{EmailMessage}"/> representing the result of the asynchronous operation.</returns>
        Task<EmailMessage> GetNotificationMessage(string applicationName, string notificationId);

        /// <summary>
        /// Gets all email templates corresponding to the application.
        /// </summary>
        /// <param name="applicationName">.</param>
        /// <returns>list of MailTemplateInfo <see cref="MailTemplateInfo"/>.</returns>
        Task<IList<MailTemplateInfo>> GetAllTemplateEntities(string applicationName);

        /// <summary>
        /// Gets the applications configured in notification service.
        /// </summary>
        /// <returns>List of applications.</returns>
        IList<string> GetApplications();

        /// <summary>
        /// Gets the report notifications for meeting invites depending on the input.
        /// </summary>
        /// <param name="notificationReportRequest"> notification filter request. </param>
        /// <returns> notifications filtered based on input.</returns>
        Task<Tuple<IList<MeetingInviteReportResponse>, TableContinuationToken>> GetMeetingInviteReportNotifications(NotificationReportRequest notificationReportRequest);
    }
}
