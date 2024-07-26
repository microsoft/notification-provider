namespace NotificationService.UnitTests.Data.CloudStorage
{
    using Azure.Storage.Blobs;
    using Moq;
    using NotificationService.Common;
    using NotificationService.Data;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NotificationService.Common.Logger;
    using Azure.Storage.Queues;
    using Microsoft.Extensions.Options;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class CloudStorageClientSetup
    {
        public CloudStorageClient SUT;
        public IOptions<StorageAccountSetting> storageAccountSetting;
        public ConcurrentDictionary<string, BlobContainerClient> blobContainerClientCD;
        public Mock<BlobContainerClient> blobContainerClient;
        public static ConcurrentDictionary<string, BlobServiceClient> blobClientRefs;
        public Mock<QueueClient> cloudQueueClient { get; set; }
        public Mock<BlobServiceClient> blobServiceClient;
        public Mock<BlobClient> blobClient;
        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        public readonly Mock<ILogger> logger;

        public CloudStorageClientSetup()
        {
            PrepareTestData();
            this.logger = new Mock<ILogger>();
            this.cloudQueueClient = new Mock<QueueClient>();     
            this.blobServiceClient = new Mock<BlobServiceClient>();
            this.blobContainerClient = new Mock<BlobContainerClient>();
            this.blobClient = new Mock<BlobClient>();
            InjectMocks();
        }


        private void PrepareTestData()
        {
            this.storageAccountSetting = Options.Create<StorageAccountSetting>(new StorageAccountSetting
            {
                BlobContainerName = "Test",
                MailTemplateTableName = "MailTemplate",
                EmailHistoryTableName = "EmailHistory",
                MeetingHistoryTableName = "MeetingHistory",
                NotificationQueueName = "test-queue",
                StorageAccountName = "Test Storage",
                StorageTableAccountURI = "https://teststorageaccounturi.table.core.windows.net",
                StorageBlobAccountURI = "https://teststorageaccounturi.blob.core.windows.net",
                StorageQueueAccountURI = "https://teststorageaccounturi.queue.core.windows.net"
            });
        }

        private void InjectMocks()
        {            
            SUT = new CloudStorageClient(this.storageAccountSetting, this.logger.Object);
        }
    }
}
