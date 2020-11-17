// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebNotifications.Hubs.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts.Models.Web.Response;

    /// <summary>
    /// The <see cref="INotificationsClient"/> interface provides mechanism for clients to respond to the web notifications.
    /// </summary>
    public interface INotificationsClient
    {
        /// <summary>
        /// Receives the notifications asynchronously.
        /// </summary>
        /// <param name="notifications">The instance for <see cref="IEnumerable{WebNotification}"/>.</param>
        /// <returns>The instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        Task ReceiveNotificationsAsync(IEnumerable<WebNotification> notifications);

        /// <summary>
        /// Receives the notification asynchronously.
        /// </summary>
        /// <param name="notification">The instance of <see cref="WebNotification"/>.</param>
        /// <returns>The instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        Task ReceiveNotificationAsync(WebNotification notification);
    }
}
