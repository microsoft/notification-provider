// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Interface to Azure Cloud Storage.
    /// </summary>
    public interface ITableStorageClient
    {
        /// <summary>
        /// Gets an instance of <see cref="CloudTable"/> for the input queue name.
        /// </summary>
        /// <param name="tableName">Name of queue.</param>
        /// <returns><see cref="CloudTable"/>.</returns>
        CloudTable GetCloudTable(string tableName);

        /// <summary>
        /// Queues the input messages to the input Cloud Queue.
        /// </summary>
        /// <param name="cloudQueue">Cloud Queue to which the messages to be pushed.</param>
        /// <param name="messages">List of messages (serialized) to be queued.</param>
        /// <param name="initialVisibilityDelay">(Optional) Time after which the message(s) should appear in Storage queue.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        // Task QueueCloudMessages(CloudQueue cloudQueue, IEnumerable<string> messages, TimeSpan? initialVisibilityDelay = null);
    }
}
