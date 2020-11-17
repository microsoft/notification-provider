// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebNotifications.Carriers.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts.Models.Web.Response;

    /// <summary>
    /// The <see cref="IWebNotificationsCarrier"/> interfaces provides a mechanism to send the notifications.
    /// </summary>
    public interface IWebNotificationsCarrier
    {
        /// <summary>
        /// Sends the notifications asynchronously.
        /// </summary>
        /// <param name="webNotifications">The instance for <see cref="IEnumerable{WebNotification}"/>.</param>
        /// <returns>The instance of <see cref="Task{T}"/>, where <c>T</c> being <see cref="IEnumerable{String}"/>, representing an asynchronous operation.</returns>
        Task<IEnumerable<string>> SendAsync(IEnumerable<WebNotification> webNotifications);
    }
}
