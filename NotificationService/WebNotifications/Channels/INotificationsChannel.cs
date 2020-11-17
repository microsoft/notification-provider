// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebNotifications.Channels
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NotificationService.Contracts.Models.Web.Response;

    /// <summary>
    /// The <see cref="INotificationsChannel"/> interface provides producer/consumer functionality for notifications.
    /// </summary>
    public interface INotificationsChannel
    {
        /// <summary>
        /// Adds the notification asynchronously.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The instance of <see cref="Task{Boolean}"/> representing an asynchronous operation.</returns>
        Task<bool> AddNotificationAsync(WebNotification notification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads all notifications asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The instance of <see cref="IAsyncEnumerable{WebNotification}"/>.</returns>
        IAsyncEnumerable<WebNotification> ReadAllNotificationsAsync(CancellationToken cancellationToken = default);
    }
}