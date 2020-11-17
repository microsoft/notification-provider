// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Trackers
{
    using System.Collections.Generic;

    /// <summary>
    /// The <see cref="IUserConnectionsReader"/> interface provides mechanism to read connection information.
    /// </summary>
    public interface IUserConnectionsReader
    {
        /// <summary>
        /// Gets the user connection ids.
        /// </summary>
        /// <param name="userObjectIdentifier">The user object identifier.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>The user connection Ids.</returns>
        IEnumerable<string> GetUserConnectionIds(string userObjectIdentifier, string applicationName);
    }
}
