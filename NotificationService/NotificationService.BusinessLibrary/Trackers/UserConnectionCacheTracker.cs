// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Trackers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Logging;
    using NotificationService.Contracts.Models.Trackers;

    /// <summary>
    /// The <see cref="UserConnectionCacheTracker"/> class tracks the user connection information in cache.
    /// </summary>
    /// <seealso cref="IUserConnectionTracker" />
    public class UserConnectionCacheTracker : IUserConnectionTracker, IUserConnectionsReader
    {
        /// <summary>
        /// The browser application.
        /// </summary>
        public const string BrowserApplication = "Browser";

        /// <summary>
        /// The distributed cache.
        /// </summary>
        private readonly IDistributedCache distributedCache;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<UserConnectionTracker> logger;

        /// <summary>
        /// The lock object.
        /// </summary>
        private readonly object lockObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserConnectionCacheTracker"/> class (DI usage: Singleton only).
        /// </summary>
        /// <remarks>
        /// DI usage: Singleton only.
        /// </remarks>
        /// <param name="distributedCache">The instance for <see cref="IDistributedCache"/>.</param>
        /// <param name="logger">The instance of <see cref="ILogger{UserConnectionTracker}"/>.</param>
        /// <exception cref="ArgumentNullException">logger.</exception>
        public UserConnectionCacheTracker(IDistributedCache distributedCache, ILogger<UserConnectionTracker> logger)
        {
            this.distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.lockObject = new object();
        }

        /// <inheritdoc />
        public HashSet<UserConnectionInfo> RetrieveConnectionInfo(string userObjectIdentifier)
        {
            HashSet<UserConnectionInfo> userConnectionsSet = null;
            this.logger.LogInformation($"Started {nameof(this.RetrieveConnectionInfo)} method of {nameof(UserConnectionTracker)}.");
            if (string.IsNullOrWhiteSpace(userObjectIdentifier))
            {
                throw new ArgumentException("The user object identifier is not specified.", nameof(userObjectIdentifier));
            }

            userConnectionsSet = Task.Run(async () => await this.GetCacheEntryAsync(userObjectIdentifier).ConfigureAwait(false)).Result;
            this.logger.LogInformation($"Finished {nameof(this.RetrieveConnectionInfo)} method of {nameof(UserConnectionTracker)}.");
            return userConnectionsSet;
        }

        /// <inheritdoc />
        public void SetConnectionInfo(string userObjectIdentifier, UserConnectionInfo userConnectionInfo)
        {
            this.logger.LogInformation($"Started {nameof(this.SetConnectionInfo)} method of {nameof(UserConnectionTracker)}.");
            if (string.IsNullOrWhiteSpace(userObjectIdentifier))
            {
                throw new ArgumentException("The user object identifier is not specified.", nameof(userObjectIdentifier));
            }

            if (userConnectionInfo == null)
            {
                throw new ArgumentNullException(nameof(userConnectionInfo));
            }

            this.AddConnection(userObjectIdentifier, userConnectionInfo);
            this.logger.LogInformation($"Finished {nameof(this.SetConnectionInfo)} method of {nameof(UserConnectionTracker)}.");
        }

        /// <inheritdoc />
        public void RemoveConnectionInfo(string userObjectIdentifier, string connectionId)
        {
            this.logger.LogInformation($"Started {nameof(this.RemoveConnectionInfo)} method of {nameof(UserConnectionTracker)}.");
            if (string.IsNullOrWhiteSpace(userObjectIdentifier))
            {
                throw new ArgumentException("The user object identifier is not specified.", nameof(userObjectIdentifier));
            }

            if (string.IsNullOrWhiteSpace(connectionId))
            {
                throw new ArgumentException("The connection Id is not specified.", nameof(connectionId));
            }

            this.RemoveConnectionInternal(userObjectIdentifier, connectionId);
            this.logger.LogInformation($"Finished {nameof(this.RemoveConnectionInfo)} method of {nameof(UserConnectionTracker)}.");
        }

        /// <inheritdoc />
        public void SetConnectionApplicationName(string userObjectIdentifier, UserConnectionInfo userConnectionInfo)
        {
            this.logger.LogInformation($"Started {nameof(this.SetConnectionApplicationName)} method of {nameof(UserConnectionTracker)}.");
            if (string.IsNullOrWhiteSpace(userObjectIdentifier))
            {
                throw new ArgumentException("The user object identifier is not specified.", nameof(userObjectIdentifier));
            }

            if (userConnectionInfo == null)
            {
                throw new ArgumentNullException(nameof(userConnectionInfo));
            }

            this.SetApplicationNameInternal(userObjectIdentifier, userConnectionInfo);
            this.logger.LogInformation($"Finished {nameof(this.SetConnectionApplicationName)} method of {nameof(UserConnectionTracker)}.");
        }

        /// <inheritdoc />
        public IEnumerable<string> GetUserConnectionIds(string userObjectIdentifier, string applicationName)
        {
            IEnumerable<string> connectionIds;
            this.logger.LogInformation($"Started {nameof(this.GetUserConnectionIds)} method of {nameof(UserConnectionTracker)}.");
            if (string.IsNullOrWhiteSpace(userObjectIdentifier))
            {
                throw new ArgumentException("The user object identifier is not specified.", nameof(userObjectIdentifier));
            }

            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("The application name is not specified.", nameof(applicationName));
            }

            connectionIds = this.GetConnectionIdsInternal(userObjectIdentifier, applicationName);
            this.logger.LogInformation($"Finished {nameof(this.GetUserConnectionIds)} method of {nameof(UserConnectionTracker)}.");
            return connectionIds;
        }

        /// <summary>
        /// Gets the connection ids internally for given userObjectIdentifier and application.
        /// </summary>
        /// <param name="userObjectIdentifier">The user object identifier.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>The instance of <see cref="IEnumerable{String}"/>.</returns>
        private IEnumerable<string> GetConnectionIdsInternal(string userObjectIdentifier, string applicationName)
        {
            List<string> connectionIds = new List<string>();
            HashSet<UserConnectionInfo> userConnectionsSet = null;
            lock (this.lockObject)
            {
                // lock keyword cannot have await in its block.
                userConnectionsSet = Task.Run(async () => await this.GetCacheEntryAsync(userObjectIdentifier).ConfigureAwait(false)).Result;
                if (userConnectionsSet != null)
                {
                    foreach (var userConnectionInfo in userConnectionsSet)
                    {
                        if (userConnectionInfo.ApplicationName.Equals(applicationName, StringComparison.Ordinal)
                            || !userConnectionInfo.ApplicationName.Equals(UserConnectionTracker.BrowserApplication, StringComparison.Ordinal))
                        {
                            connectionIds.Add(userConnectionInfo.ConnectionId);
                        }
                    }
                }
            }

            return connectionIds;
        }

        /// <summary>
        /// Sets the application name internal.
        /// </summary>
        /// <param name="userObjectIdentifier">The user object identifier.</param>
        /// <param name="userConnectionInfo">The instance of <see cref="UserConnectionInfo"/>..</param>
        private void SetApplicationNameInternal(string userObjectIdentifier, UserConnectionInfo userConnectionInfo)
        {
            HashSet<UserConnectionInfo> userConnectionsSet;
            lock (this.lockObject)
            {
                // lock keyword cannot have await in its block.
                userConnectionsSet = Task.Run(async () => await this.GetCacheEntryAsync(userObjectIdentifier).ConfigureAwait(false)).Result;
                if (userConnectionsSet != null)
                {
                    _ = userConnectionsSet.Remove(userConnectionInfo);
                }
                else
                {
                    userConnectionsSet = new HashSet<UserConnectionInfo>();
                }

                _ = userConnectionsSet.Add(userConnectionInfo);
                Task.Run(async () => await this.SetCacheEntryAsync(userObjectIdentifier, userConnectionsSet).ConfigureAwait(false)).Wait();
            }
        }

        /// <summary>
        /// Removes the connection privately.
        /// </summary>
        /// <param name="userObjectIdentifier">The user object identifier.</param>
        /// <param name="connectionId">The connection identifier.</param>
        private void RemoveConnectionInternal(string userObjectIdentifier, string connectionId)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(userObjectIdentifier), "userObjectIdentifier is invalid.");
            Debug.Assert(!string.IsNullOrWhiteSpace(connectionId), "connectionId is invalid.");
            UserConnectionInfo userConnectionInfo = new UserConnectionInfo(connectionId);
            HashSet<UserConnectionInfo> userConnectionsSet;
            lock (this.lockObject)
            {
                // lock keyword cannot have await in its block.
                userConnectionsSet = Task.Run(async () => await this.GetCacheEntryAsync(userObjectIdentifier).ConfigureAwait(false)).Result;
                if (userConnectionsSet?.Any() ?? false)
                {
                    if (userConnectionsSet.Count > 1)
                    {
                        _ = userConnectionsSet.Remove(userConnectionInfo);
                        Task.Run(async () => await this.SetCacheEntryAsync(userObjectIdentifier, userConnectionsSet).ConfigureAwait(false)).Wait();
                    }
                    else
                    {
                        Task.Run(async () => await this.distributedCache.RemoveAsync(userObjectIdentifier).ConfigureAwait(false)).Wait();
                    }
                }
            }
        }

        /// <summary>
        /// Adds the connection to the dictionary.
        /// </summary>
        /// <param name="userObjectIdentifier">The user object identifier.</param>
        /// <param name="userConnectionInfo">The instance of <see cref="UserConnectionInfo"/>.</param>
        private void AddConnection(string userObjectIdentifier, UserConnectionInfo userConnectionInfo)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(userObjectIdentifier), "UserObjectIdentifier is null.");
            Debug.Assert(userConnectionInfo != null, "The userConnectionInfo is null.");
            HashSet<UserConnectionInfo> userConnectionsSet;
            lock (this.lockObject)
            {
                // lock keyword cannot have await in its block.
                userConnectionsSet = Task.Run(async () => await this.GetCacheEntryAsync(userObjectIdentifier).ConfigureAwait(false)).Result;
                if (userConnectionsSet == null)
                {
                    userConnectionsSet = new HashSet<UserConnectionInfo>();
                }

                _ = userConnectionsSet.Add(userConnectionInfo);
                Task.Run(async () => await this.SetCacheEntryAsync(userObjectIdentifier, userConnectionsSet).ConfigureAwait(false)).Wait();
            }
        }

        /// <summary>
        /// Gets the cache entry asynchronously.
        /// </summary>
        /// <param name="key">The cahce key.</param>
        /// <returns>THe instance of <see cref="Task{T}"/> with <c>T</c> being <see cref="HashSet{UserConnectionInfo}"/>.</returns>
        private async Task<HashSet<UserConnectionInfo>> GetCacheEntryAsync(string key)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(key), "Missing key");
            HashSet<UserConnectionInfo> userConnectionsSet = null;
            byte[] cachedBytes = await this.distributedCache.GetAsync(key).ConfigureAwait(false);
            if (cachedBytes != null)
            {
                string serializedValue = Encoding.UTF8.GetString(cachedBytes);
                userConnectionsSet = JsonSerializer.Deserialize<HashSet<UserConnectionInfo>>(serializedValue);
            }

            return userConnectionsSet;
        }

        /// <summary>
        /// Sets the cache entry asynchronously.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="userConnectionsSet">The instance of <see cref="HashSet{UserConnectionInfo}"/>.</param>
        /// <returns>The instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        private async Task SetCacheEntryAsync(string key, HashSet<UserConnectionInfo> userConnectionsSet)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(key), "Missing key");
            Debug.Assert(userConnectionsSet != null, "Missing Connection Set");
            string jsonSerializedValue = JsonSerializer.Serialize(userConnectionsSet);
            byte[] tobeCachedBytes = Encoding.UTF8.GetBytes(jsonSerializedValue);
            await this.distributedCache.SetAsync(key, tobeCachedBytes).ConfigureAwait(false);
        }
    }
}
