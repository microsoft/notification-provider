// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IRepositoryFactory
    {
        public IEmailNotificationRepository GetRepository(StorageType type);
    }
}
