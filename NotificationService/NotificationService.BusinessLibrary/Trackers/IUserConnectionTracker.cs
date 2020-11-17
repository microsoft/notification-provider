// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Trackers
{
    using System.Collections.Generic;
    using NotificationService.Contracts.Models.Trackers;

    /// <summary>
    /// The <see cref="IUserConnectionTracker"/> provides mechanism to track user SIgnalR connections.
    /// </summary>
    public interface IUserConnectionTracker
    {
        /// <summary>
        /// Retrieves the connection information.
        /// </summary>
        /// <param name="userObjectIdentifier">The user object identifier.</param>
        /// <returns>The instance of <see cref="HashSet{UserConnectionInfo}"/>.</returns>
        HashSet<UserConnectionInfo> RetrieveConnectionInfo(string userObjectIdentifier);

        /// <summary>
        /// Sets the connection information for the specified user object identifier.
        /// </summary>
        /// <param name="userObjectIdentifier">The user object identifier.</param>
        /// <param name="userConnectionInfo">The instance of <see cref="UserConnectionInfo"/>.</param>
        void SetConnectionInfo(string userObjectIdentifier, UserConnectionInfo userConnectionInfo);

        /// <summary>
        /// Removes the connection information from tracker.
        /// </summary>
        /// <param name="userObjectIdentifier">The user object identifier.</param>
        /// <param name="connectionId">The user connection identifier.</param>
        void RemoveConnectionInfo(string userObjectIdentifier, string connectionId);

        /// <summary>
        /// Sets the name of the application for a connection.
        /// </summary>
        /// <param name="userObjectIdentifier">The user object identifier.</param>
        /// <param name="userConnectionInfo">The instance of <see cref="UserConnectionInfo"/>.</param>
        void SetConnectionApplicationName(string userObjectIdentifier, UserConnectionInfo userConnectionInfo);
    }
}
