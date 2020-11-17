// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Storage.Queue;

    /// <summary>
    /// Interface to Azure Cloud Storage.
    /// </summary>
    public interface ICloudStorageClient
    {
        /// <summary>
        /// Gets an instance of <see cref="CloudQueue"/> for the input queue name.
        /// </summary>
        /// <param name="queueName">Name of queue.</param>
        /// <returns><see cref="CloudQueue"/>.</returns>
        CloudQueue GetCloudQueue(string queueName);

        /// <summary>
        /// Queues the input messages to the input Cloud Queue.
        /// </summary>
        /// <param name="cloudQueue">Cloud Queue to which the messages to be pushed.</param>
        /// <param name="messages">List of messages (serialized) to be queued.</param>
        /// <param name="initialVisibilityDelay">(Optional) Time after which the message(s) should appear in Storage queue.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task QueueCloudMessages(CloudQueue cloudQueue, IEnumerable<string> messages, TimeSpan? initialVisibilityDelay = null);

        /// <summary>
        /// Uploads the content to the blob.
        /// </summary>
        /// <param name="blobName">Blob name.</param>
        /// <param name="content">Blob content.</param>
        /// <returns>Blob Uri.</returns>
        Task<string> UploadBlobAsync(string blobName, string content);

        /// <summary>
        /// Downloads the content from the blob as a stream.
        /// </summary>
        /// <param name="blobName">Blob name.</param>
        /// <returns>Blob content.</returns>
        Task<string> DownloadBlobAsync(string blobName);

        /// <summary>
        /// Deletes the blob.
        /// </summary>
        /// <param name="blobName">Blob name.</param>
        /// <returns>Returns true if delete suceeded.</returns>
        Task<bool> DeleteBlobsAsync(string blobName);
    }
}
