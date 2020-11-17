// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts;

    /// <summary>
    /// Interface for Notification Service Manager.
    /// </summary>
    public interface IEmailServiceManager
    {
        /// <summary>
        /// Process email notification items.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="emailNotificationItems">Array of email notification items.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<IList<NotificationResponse>> SendEmailNotifications(string applicationName, EmailNotificationItem[] emailNotificationItems);

        /// <summary>
        /// Process email notification items.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="queueNotificationItem">Queue notification item to be processed.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<IList<NotificationResponse>> ProcessEmailNotifications(string applicationName, QueueNotificationItem queueNotificationItem);
    }
}
