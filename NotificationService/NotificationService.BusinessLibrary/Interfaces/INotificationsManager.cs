// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts.Models.Web.Request;
    using NotificationService.Contracts.Models.Web.Response;

    /// <summary>
    /// The <see cref="INotificationsManager"/> interface provides mechanism to work with notifications.
    /// </summary>
    public interface INotificationsManager
    {
        /// <summary>
        /// Processes the notifications asynchronously.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="webNotificationRequestItems">The instance for <see cref="IEnumerable{WebNotificationRequestItem}"/>.</param>
        /// <returns>An instance of <see cref="Task{WebNotification}"/> representing an asynchronous operation.</returns>
        Task<IEnumerable<WebNotification>> ProcessNotificationsAsync(string applicationName, IEnumerable<WebNotificationRequestItem> webNotificationRequestItems);

        /// <summary>
        /// Marks the notifications' read status asynchronously.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="notificationIds">The instance for <see cref="IEnumerable{string}"/>.</param>
        /// <returns>An instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        Task MarkNotificationsAsReadAsync(string applicationName, IEnumerable<string> notificationIds);

        /// <summary>
        /// Delivers the notifications asynchronously.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="userObjectId">The user object identifier.</param>
        /// <returns>An instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        Task<WebNotificationResponse> DeliverNotificationsAsync(string applicationName, string userObjectId);

        /// <summary>
        /// Loads the notification status asynchronously.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="trackingIds">The tracking ids.</param>
        /// <returns>The instance of <see cref="Task{WebNotificationStatusResponse}"/>.</returns>
        Task<WebNotificationStatusResponse> LoadNotificationStatusAsync(string applicationName, IEnumerable<string> trackingIds);
    }
}
