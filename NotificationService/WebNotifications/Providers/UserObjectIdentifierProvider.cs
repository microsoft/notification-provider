// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebNotifications.Providers
{
    using System;
    using Microsoft.AspNetCore.SignalR;
    using NotificationService.SvCommon.APIConstants;

    /// <summary>
    /// The <see cref="UserObjectIdentifierProvider"/> class implements mechanism to identify user using object identifier.
    /// </summary>
    /// <seealso cref="IUserIdProvider" />
    public class UserObjectIdentifierProvider : IUserIdProvider
    {
        /// <summary>
        /// Gets the user ID for the specified SignalR connection context.
        /// </summary>
        /// <param name="connection">The instance of <see cref="HubConnectionContext"/>.</param>
        /// <returns>
        /// The user object identifier for the specified connection context.
        /// </returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public string GetUserId(HubConnectionContext connection)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            string userObjectIdentifier = connection.User.FindFirst("oid")?.Value;
            if (string.IsNullOrWhiteSpace(userObjectIdentifier))
            {
                userObjectIdentifier = connection.User.FindFirst(ClaimTypeConstants.ObjectIdentifier)?.Value;
            }

            return userObjectIdentifier;
        }
    }
}
