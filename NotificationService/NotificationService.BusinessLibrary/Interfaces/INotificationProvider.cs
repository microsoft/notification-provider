// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NotificationService.Contracts;

    /// <summary>
    /// The Notification provider Interface.
    /// </summary>
    public interface INotificationProvider
    {
        /// <summary>
        /// Process notifiication entities using corresponding provider.
        /// </summary>
        /// <param name="applicationName"> applicationName sourcing the notifications.</param>
        /// <param name="notificationEntities">List of notification entities that are to be processed.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task ProcessNotificationEntities(string applicationName, IList<EmailNotificationItemEntity> notificationEntities);
    }
}
