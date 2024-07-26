// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Azure.Storage.Queues.Models;

    /// <summary>
    /// Interface to Azure Cloud Storage.
    /// </summary>
    public interface ICloudStorageClient
    {
        /// <summary>
        /// Queues the input messages to the input Cloud Queue.
        /// </summary>
        /// <param name="cloudQueue">Cloud Queue to which the messages to be pushed.</param>
        /// <param name="messages">List of messages (serialized) to be queued.</param>
        /// <param name="initialVisibilityDelay">(Optional) Time after which the message(s) should appear in Storage queue.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task QueueCloudMessages(IEnumerable<string> messages);

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
