namespace NotificationService.UnitTests.Data.CloudStorage
{
    using Azure;
    using Azure.Data.Tables;
    using Azure.Data.Tables.Models;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Microsoft.Extensions.Options;
    using Moq;
    using NotificationService.Common;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models;
    using NotificationService.Data;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    [TestFixture]
    public class TableStorageClientTests
    {
        private TableStorageClient _tableStorageClient;
        private Mock<TableServiceClient> _mockTableServiceClient;
        private Mock<TableClient> _mockTableClient;
        private Mock<ITableStorageClient> _mockTableStorageClient;
        public IOptions<StorageAccountSetting> storageAccountSetting;

        [SetUp]
        public void Setup()
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

            _mockTableStorageClient = new Mock<ITableStorageClient>();
            _mockTableServiceClient = new Mock<TableServiceClient>(MockBehavior.Strict);
            _mockTableClient = new Mock<TableClient>(MockBehavior.Strict);
            _mockTableServiceClient.Setup(c => c.GetTableClient(It.IsAny<string>())).Returns(_mockTableClient.Object);
            _tableStorageClient = new TableStorageClient(new Microsoft.Extensions.Options.OptionsWrapper<StorageAccountSetting>(
                new StorageAccountSetting { StorageTableAccountURI = "https://dummyaccount.table.core.windows.net/" }));
        }

        [Test]
        public void GetTableServiceClient_ShouldReturnTableServiceClient()
        {
            var storageAccountUri = "https://dummyaccount.table.core.windows.net/";
            var result = _tableStorageClient.GetTableServiceClient(storageAccountUri);
            Assert.NotNull(result);
        }

    }
}
