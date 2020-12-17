// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Queue;
    using Microsoft.Extensions.Options;
    using NotificationService.Common;
    using NotificationService.Common.Logger;

    /// <summary>
    /// Client Interface to the Azure Cloud Storage.
    /// </summary>
    public class CloudStorageClient : ICloudStorageClient
    {
        /// <summary>
        /// Instance of <see cref="StorageAccountSetting"/>.
        /// </summary>
        private readonly StorageAccountSetting storageAccountSetting;

        /// <summary>
        /// Instance of <see cref="CloudStorageAccount"/>.
        /// </summary>
        private readonly CloudStorageAccount cloudStorageAccount;

        /// <summary>
        /// Instance of <see cref="CloudQueueClient"/>.
        /// </summary>
        private readonly CloudQueueClient cloudQueueClient;

        /// <summary>
        /// Instance of <see cref="BlobContainerClient"/>.
        /// </summary>
        private readonly BlobContainerClient blobContainerClient;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of <see cref="BlobContainerClient"/>.
        /// </summary>
        private BlobContainerClient attachmentBlobContainerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageClient"/> class.
        /// </summary>
        /// <param name="storageAccountSetting">Storage Account configuration.</param>
        /// <param name="logger"><see cref="ILogger"/> instance.</param>
        public CloudStorageClient(IOptions<StorageAccountSetting> storageAccountSetting, ILogger logger)
        {
            this.storageAccountSetting = storageAccountSetting?.Value;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cloudStorageAccount = CloudStorageAccount.Parse(this.storageAccountSetting.ConnectionString);
            this.cloudQueueClient = this.cloudStorageAccount.CreateCloudQueueClient();
            this.blobContainerClient = new BlobContainerClient(this.storageAccountSetting.ConnectionString, this.storageAccountSetting.BlobContainerName);
            if (!this.blobContainerClient.Exists())
            {
                this.logger.TraceWarning($"BlobStorageClient - Method: {nameof(CloudStorageClient)} - No container found with name {this.storageAccountSetting.BlobContainerName}.");

                var response = this.blobContainerClient.CreateIfNotExists();

                this.blobContainerClient = new BlobContainerClient(this.storageAccountSetting.ConnectionString, this.storageAccountSetting.BlobContainerName);
            }
        }

        /// <inheritdoc/>
        public CloudQueue GetCloudQueue(string queueName)
        {
            CloudQueue cloudQueue = this.cloudQueueClient.GetQueueReference(queueName);
            _ = cloudQueue.CreateIfNotExists();
            return cloudQueue;
        }

        /// <inheritdoc/>
        public Task QueueCloudMessages(CloudQueue cloudQueue, IEnumerable<string> messages, TimeSpan? initialVisibilityDelay = null)
        {
            messages.ToList().ForEach(msg =>
            {
                CloudQueueMessage message = new CloudQueueMessage(msg);
                cloudQueue.AddMessage(message, null, initialVisibilityDelay);
            });
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<string> UploadBlobAsync(string blobName, string content)
        {
            BlobClient blobClient = this.blobContainerClient.GetBlobClient(blobName);
            var contentBytes = Convert.FromBase64String(content);
            using (var stream = new MemoryStream(contentBytes))
            {
                var result = await blobClient.UploadAsync(stream, overwrite: true).ConfigureAwait(false);
            }

            return string.Concat(this.blobContainerClient.Uri, "/", blobName);
        }

        /// <inheritdoc/>
        public async Task<string> UploadAttachmentToBlobAsync(string applicationName, string blobPath, string content)
        {
            this.GetAttachmentBlobContainerClient(applicationName);

            BlobClient blobClient = this.attachmentBlobContainerClient.GetBlobClient(blobPath);
            var contentBytes = Convert.FromBase64String(content);
            using (var stream = new MemoryStream(contentBytes))
            {
                var result = await blobClient.UploadAsync(stream, overwrite: true).ConfigureAwait(false);
            }

            return blobPath;
        }

        /// <inheritdoc/>
        public async Task<string> DownloadBlobAsync(string blobName)
        {
            BlobClient blobClient = this.blobContainerClient.GetBlobClient(blobName);
            bool isExists = await blobClient.ExistsAsync().ConfigureAwait(false);
            if (isExists)
            {
                var blob = await blobClient.DownloadAsync().ConfigureAwait(false);
                byte[] streamArray = new byte[blob.Value.ContentLength];
                long numBytesToRead = blob.Value.ContentLength;
                int numBytesRead = 0;
                int maxBytesToRead = 10;
                do
                {
                    if (numBytesToRead < maxBytesToRead)
                    {
                        maxBytesToRead = (int)numBytesToRead;
                    }

                    int n = blob.Value.Content.Read(streamArray, numBytesRead, maxBytesToRead);
                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                while (numBytesToRead > 0);

                return Convert.ToBase64String(streamArray);
            }
            else
            {
                this.logger.TraceWarning($"BlobStorageClient - Method: {nameof(this.DownloadBlobAsync)} - No blob found with name {blobName}.");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<string> DownloadAttachmentFromBlobAsync(string applicationName, string blobPath)
        {
            this.GetAttachmentBlobContainerClient(applicationName);

            BlobClient blobClient = this.attachmentBlobContainerClient.GetBlobClient(blobPath);
            bool isExists = await blobClient.ExistsAsync().ConfigureAwait(false);
            if (isExists)
            {
                var blob = await blobClient.DownloadAsync().ConfigureAwait(false);
                byte[] streamArray = new byte[blob.Value.ContentLength];
                long numBytesToRead = blob.Value.ContentLength;
                int numBytesRead = 0;
                int maxBytesToRead = 10;
                do
                {
                    if (numBytesToRead < maxBytesToRead)
                    {
                        maxBytesToRead = (int)numBytesToRead;
                    }

                    int n = blob.Value.Content.Read(streamArray, numBytesRead, maxBytesToRead);
                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                while (numBytesToRead > 0);

                return Convert.ToBase64String(streamArray);
            }
            else
            {
                this.logger.TraceWarning($"BlobStorageClient - Method: {nameof(this.DownloadAttachmentFromBlobAsync)} - No blob found with name {blobPath}.");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteBlobsAsync(string blobName)
        {
            BlobClient blobClient = this.blobContainerClient.GetBlobClient(blobName);
            bool isExists = await blobClient.ExistsAsync().ConfigureAwait(false);
            if (isExists)
            {
                var response = await blobClient.DeleteAsync().ConfigureAwait(false);
                return true;
            }
            else
            {
                this.logger.TraceWarning($"BlobStorageClient - Method: {nameof(this.DeleteBlobsAsync)} - No blob found with name {blobName}.");
                return false;
            }
        }

        private void GetAttachmentBlobContainerClient(string containerName)
        {
            string container = containerName?.ToLowerInvariant();
            this.attachmentBlobContainerClient = new BlobContainerClient(this.storageAccountSetting.ConnectionString, container);
            if (!this.attachmentBlobContainerClient.Exists())
            {
                this.logger.TraceWarning($"BlobStorageClient - Method: {nameof(this.GetAttachmentBlobContainerClient)} - No container found with name {containerName}.");

                var response = this.attachmentBlobContainerClient.CreateIfNotExists();

                this.attachmentBlobContainerClient = new BlobContainerClient(this.storageAccountSetting.ConnectionString, container);
            }
        }
    }
}
