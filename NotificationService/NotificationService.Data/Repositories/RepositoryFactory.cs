// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data.Repositories
{
    using System;
    using NotificationService.Data.Interfaces;

    /// <summary>
    /// A factory class to return an instance of CosmosDb repository or Storage Account repository as per argument passed as StorageType.
    /// </summary>
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider"> <see cref="IServiceProvider"/>.</param>
        public RepositoryFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// A factory method for getting Repository instance based on type.
        /// </summary>
        /// <param name="type">StorageType parameter for type decision.</param>
        /// <returns><see cref="IEmailNotificationRepository"></see>.</returns>
        public IEmailNotificationRepository GetRepository(StorageType type)
        {
            switch (type)
            {
                //case StorageType.DocumentDB:
                //    return (IEmailNotificationRepository)this.serviceProvider.GetService(typeof(EmailNotificationRepository));
                case StorageType.StorageAccount:
                    return (IEmailNotificationRepository)this.serviceProvider.GetService(typeof(TableStorageEmailRepository));
                default:
                    return null;
            }
        }
    }
}
