// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NotificationService.Common;
    using NotificationService.Data.Interfaces;

    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IServiceProvider serviceProvider;

        public RepositoryFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEmailNotificationRepository GetRepository(StorageType type)
        {
            switch (type)
            {
                case StorageType.DocumentDB:
                    return (IEmailNotificationRepository)this.serviceProvider.GetService(typeof(EmailNotificationRepository));
                case StorageType.StorageAccount:
                    return (IEmailNotificationRepository)this.serviceProvider.GetService(typeof(TableStorageEmailRepository));
                default:
                    return null;
            }
        }
    }
}
