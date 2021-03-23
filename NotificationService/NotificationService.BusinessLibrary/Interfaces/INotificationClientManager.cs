// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for NotificationClient request service.
    /// </summary>
    public interface INotificationClientManager
    {
        /// <summary>
        /// Gets the applications configured in notification service.
        /// </summary>
        /// <returns>List of applications.</returns>
        IList<string> GetApplications();
    }
}
