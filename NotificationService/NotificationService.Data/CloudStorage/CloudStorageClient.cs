// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using Azure;
    using Azure.Core;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.Options;
    using Azure.Storage.Queues.Models;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage;
    using Azure.Storage.Queues;
    using NotificationService.Data.Helper;
    using System.Text;

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
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;
        private QueueClient cloudQueueClient { get; set; }
        //private IKVHelper kVHelper;
        private ConcurrentDictionary<string, BlobContainerClient> blobContainerClientCD;
        private BlobContainerClient blobContainerClient;
        private static ConcurrentDictionary<string, BlobServiceClient> blobClientRefs;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageClient"/> class.
        /// </summary>
        /// <param name="storageAccountSetting">Storage Account configuration.</param>
        /// <param name="logger"><see cref="ILogger"/> instance.</param>
        public CloudStorageClient(IOptions<StorageAccountSetting> storageAccountSetting, ILogger logger)
        {
            this.storageAccountSetting = storageAccountSetting?.Value;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cloudQueueClient = new QueueClient(new Uri(this.storageAccountSetting.StorageQueueAccountURI), AzureCredentialHelper.AzureCredentials);

            blobClientRefs = new ConcurrentDictionary<string, BlobServiceClient>();
            blobContainerClientCD = new ConcurrentDictionary<string, BlobContainerClient>();
        }

        private BlobServiceClient GetBlobClient(StorageAccountSetting blobServiceConfig)
        {
            if (!string.IsNullOrEmpty(this.storageAccountSetting.StorageBlobAccountURI))
            {
                return new BlobServiceClient(new Uri(this.storageAccountSetting.StorageBlobAccountURI), AzureCredentialHelper.AzureCredentials);
            }
            else if (!string.IsNullOrEmpty(blobServiceConfig.StorageAccountName))
            {
                string blobUri = string.Format("https://{0}.blob.core.windows.net", blobServiceConfig.StorageAccountName);
                return new BlobServiceClient(new Uri(blobUri), AzureCredentialHelper.AzureCredentials);
            }
            else
            {
                throw new Exception("Either Storagre URI or Storage Name must be provided to connect to BlobServiceClient.");
            }
        }

        /// <summary>
        /// Creation and reuse of BlobServiceClient instances by caching them based on configurations
        /// </summary>
        /// <param name="blobServiceConfig">blob service configuration entity</param>
        /// <returns> It returns the obtained or newly created BlobServiceClient instance.</returns>
        public BlobServiceClient GetBlobServiceClient(StorageAccountSetting blobServiceConfig)
        {
            BlobServiceClient blobServiceClient;
            if (!blobClientRefs.TryGetValue($"{blobServiceConfig.StorageAccountName}", out blobServiceClient))
            {
                blobServiceClient = GetBlobClient(blobServiceConfig);
                blobClientRefs.TryAdd($"{blobServiceConfig.StorageAccountName}", blobServiceClient);
            }

            return blobServiceClient;
        }

        /// <summary>
        /// Creation  and reuse of BlobContainerClient instances by caching them based on container name, 
        /// </summary>
        /// <param name="blobService">blob service client</param>
        /// <param name="containerName">container name</param>
        /// <returns></returns>
        private BlobContainerClient GetBlobContainerClient(BlobServiceClient blobService, string containerName)
        {
            BlobContainerClient blobContainerClient;
            if (!blobContainerClientCD.TryGetValue($"{blobService.AccountName}-{containerName}", out blobContainerClient))
            {
                blobContainerClient = blobService.GetBlobContainerClient(containerName);
                blobContainerClientCD.TryAdd($"{blobService.AccountName}-{containerName}", blobContainerClient);
            }

            return blobContainerClient;
        }

        /// <summary>
        /// Message, converts it to Base64 encoding, and then adds it to a mail queue asynchronously. 
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public Task QueueCloudMessages(IEnumerable<string> messages)
        {
            messages.ToList().ForEach(msg =>
            {
                msg = Convert.ToBase64String(Encoding.UTF8.GetBytes(msg));
                var res = this.cloudQueueClient.SendMessageAsync(msg);
            });

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<string> UploadBlobAsync(string blobName, string content)
        {
            Response<BlobContentInfo> blobContentInfo = null;
            BlobServiceClient blobServiceClient = GetBlobServiceClient(this.storageAccountSetting);
            blobContainerClient = GetBlobContainerClient(blobServiceClient, this.storageAccountSetting.BlobContainerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            var contentBytes = Convert.FromBase64String(content);
            using (var stream = new MemoryStream(contentBytes))
            {
                var result = await blobClient.UploadAsync(stream, overwrite: true).ConfigureAwait(false);
            }

            return string.Concat(this.blobContainerClient.Uri, "/", blobName);
        }


        /// <summary>
        /// Check the existence of blobs within Azure Blob Storage containers based on container name, blob name and configuration,
        /// </summary>
        /// <param name="containerName">Blob Container Name</param>
        /// <param name="blobName">Blob Name</param>
        /// <returns>It returns a boolean value indicating whether the blob existsor not </returns>
        public async Task<bool> CheckIfBlobExists(string containerName, string blobName, StorageAccountSetting storageAccountSetting)
        {
            bool isFound;
            try
            {
                BlobServiceClient blobServiceClient = GetBlobServiceClient(storageAccountSetting);
                blobContainerClient = GetBlobContainerClient(blobServiceClient, containerName);
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

                isFound = await blobClient.ExistsAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return isFound;
        }

        /// <summary>
        /// Retrieves the content of a blob from an Azure Blob Storage container and returns it as a string
        /// </summary>      
        /// <param name="blobName">blob name</param>
        /// <returns>It returns the content of the downloaded blob as a string.</returns>
        public async Task<string> DownloadBlobAsync(string blobName)
        {
            string content = "";
            try
            {
                if (CheckIfBlobExists(this.storageAccountSetting.BlobContainerName, blobName, this.storageAccountSetting).Result)
                {
                    BlobServiceClient blobServiceClient = GetBlobServiceClient(this.storageAccountSetting);
                    blobContainerClient = GetBlobContainerClient(blobServiceClient, this.storageAccountSetting.BlobContainerName);
                    BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
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
            catch (RequestFailedException ex)
            {
                if (ex.Status == 404)
                {
                    return "";
                }
            }
            catch (Exception)
            {
                throw;
            }

            return content;
        }

        /// <summary>
        /// Delete blobs from Azure Blob Storage, handling errors gracefully and providing feedback on the deletion status.
        /// </summary>
        /// <param name="blobName">Blob Name</param>
        /// <param name="versionId">version id timestamp</param>
        /// <returns>It returns a boolean value indicating whether the blob was successfully deleted.</returns>
        public async Task<bool> DeleteBlobsAsync( string blobName)
        {
            try
            {
                BlobServiceClient blobServiceClient = GetBlobServiceClient(this.storageAccountSetting);
                blobContainerClient = GetBlobContainerClient(blobServiceClient, this.storageAccountSetting.BlobContainerName);
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
                return await blobClient.DeleteIfExistsAsync(Azure.Storage.Blobs.Models.DeleteSnapshotsOption.None);
            }          
            catch (Exception)
            {
                throw;
            }
            return true;
        }

    }
}
