// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data.Interfaces
{
    /// <summary>
    /// Interface for the Repository Type.
    /// </summary>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// Gets the Storage provider type to use.
        /// </summary>
        /// <param name="type">Storage provider type.</param>
        /// <returns>Repository context.</returns>
        public IEmailNotificationRepository GetRepository(StorageType type);
    }
}
