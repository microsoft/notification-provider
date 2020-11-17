// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Interfaces
{
    /// <summary>
    /// The Notification provider factory Interface.
    /// </summary>
    public interface INotificationProviderFactory
    {
        /// <summary>
        /// provides the notification provider basing on type.
        /// </summary>
        /// <param name="type">NotificationProviderType.</param>
        /// <returns> return NotificationProvider <see cref="INotificationProvider"/>.</returns>
        public INotificationProvider GetNotificationProvider(NotificationProviderType type);
    }
}
