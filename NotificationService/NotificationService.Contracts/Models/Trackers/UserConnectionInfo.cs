// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Models.Trackers
{
    using System;

    /// <summary>
    /// The <see cref="UserConnectionInfo"/> class stores SignalR connection information.
    /// </summary>
    public class UserConnectionInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserConnectionInfo"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <exception cref="ArgumentException">The connection Id is not specified. - connectionId.</exception>
        public UserConnectionInfo(string connectionId, string applicationName = "")
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                throw new ArgumentException("The connection Id is not specified.", nameof(connectionId));
            }

            this.ConnectionId = connectionId;
            this.ApplicationName = applicationName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserConnectionInfo"/> class.
        /// Note :  Avoid use of this constructor in code. This is present for deserialization purpose only.
        /// </summary>
        internal UserConnectionInfo()
        {
        }

        /// <summary>
        /// Gets or sets the SignalR connection identifier.
        /// Note :  Set is available for deserialization purpose only. Avoid setting the value in code.
        /// </summary>
        /// <value>
        /// The SignalR connection identifier.
        /// </value>
        public string ConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the application from which user is connecting.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => this.ConnectionId.GetHashCode(StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Determines whether the specified <see cref="Object" />, is equal to this instance (Not a reference equality).
        /// </summary>
        /// <remarks>
        /// The <see cref="UserConnectionInfo"/> objects are equal if their connection Ids are same.
        /// </remarks>
        /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj != null && obj is UserConnectionInfo userConnectionInfo)
            {
                result = this.ConnectionId.Equals(userConnectionInfo.ConnectionId, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }
    }
}
