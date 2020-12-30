// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Options;
    using Moq;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts.Entities;
    using NotificationService.Data;
    using NotificationService.Data.Repositories;
    using NUnit.Framework;

    /// <summary>
    /// Table Storage Repository Tests Class.
    /// </summary>
    public class TableStorageRepositoryTests
    {
        /// <summary>
        /// Instance of Application Configuration.
        /// </summary>
        private readonly Mock<ITableStorageClient> cloudStorageClient;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly Mock<ILogger> logger;

        /// <summary>
        /// Instance of <see cref="IMailAttachmentRepository"/>.
        /// </summary>
        private readonly Mock<IMailAttachmentRepository> mailAttachmentRepository;

        /// <summary>
        /// Application Name.
        /// </summary>
        private readonly string applicationName = "TestApp";

        /// <summary>
        /// Instance of <see cref="meetingHistoryTable"/>.
        /// </summary>
        private Mock<CloudTable> meetingHistoryTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageRepositoryTests"/> class.
        /// </summary>
        public TableStorageRepositoryTests()
        {
            this.cloudStorageClient = new Mock<ITableStorageClient>();
            this.logger = new Mock<ILogger>();
            this.mailAttachmentRepository = new Mock<IMailAttachmentRepository>();
            this.meetingHistoryTable = new Mock<CloudTable>(new Uri("http://unittests.localhost.com/FakeTable"), (TableClientConfiguration)null);
            _ = this.cloudStorageClient.Setup(x => x.GetCloudTable("MeetingHistory")).Returns(this.meetingHistoryTable.Object);
        }

        /// <summary>
        /// Gets the meeting notification item entities tests.
        /// </summary>
        /// <returns>A Task.</returns>
        [Test]
        public async Task GetMeetingNotificationItemEntitiesTests()
        {
            IEnumerable<MeetingNotificationItemTableEntity> entities = new List<MeetingNotificationItemTableEntity> { new MeetingNotificationItemTableEntity { NotificationId = "notificationId1" }, new MeetingNotificationItemTableEntity { NotificationId = "notificationId2" } };
            IList<MeetingNotificationItemEntity> itemEntities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1" }, new MeetingNotificationItemEntity { NotificationId = "notificationId1" } };
            _ = this.mailAttachmentRepository.Setup(x => x.DownloadMeetingInvite(It.IsAny<IList<MeetingNotificationItemEntity>>(), It.IsAny<string>())).Returns(Task.FromResult(itemEntities));
            _ = this.meetingHistoryTable.Setup(x => x.ExecuteQuery(It.IsAny<TableQuery<MeetingNotificationItemTableEntity>>(), null, null)).Returns(entities);
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", ConnectionString = "Test Con", MailTemplateTableName = "MailTemplate" });
            var repo = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            var items = await repo.GetMeetingNotificationItemEntities(new List<string> { "notificationId1", "notificationId2" }, this.applicationName);
            Assert.IsTrue(items.Count == 2);
        }

        /// <summary>
        /// Gets the meeting notification item entities tests.
        /// </summary>
        /// <returns>A Task.</returns>
        [Test]
        public async Task GetEmailNotificationItemEntityTests()
        {
            IEnumerable<MeetingNotificationItemTableEntity> entities = new List<MeetingNotificationItemTableEntity> { new MeetingNotificationItemTableEntity { NotificationId = "notificationId1" } };
            _ = this.meetingHistoryTable.Setup(x => x.ExecuteQuery(It.IsAny<TableQuery<MeetingNotificationItemTableEntity>>(), null, null)).Returns(entities);
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", ConnectionString = "Test Con", MailTemplateTableName = "MailTemplate" });
            var repo = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            var items = await repo.GetMeetingNotificationItemEntity("notificationId1");
            Assert.IsTrue(items.NotificationId == "notificationId1");
        }

        /// <summary>
        /// Gets the meeting notification item entities tests.
        /// </summary>
        /// <returns>A Task.</returns>
        [Test]
        public async Task CreateMeetingNotificationItemEntitiesTests()
        {
            IList<MeetingNotificationItemEntity> entities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1", Application = "Application", RowKey = "notificationId1" }, new MeetingNotificationItemEntity { NotificationId = "notificationId2", Application = "Application", RowKey = "notificationId2" } };
            _ = this.mailAttachmentRepository.Setup(e => e.UploadMeetingInvite(It.IsAny<IList<MeetingNotificationItemEntity>>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(entities));
            this.meetingHistoryTable.Setup(x => x.ExecuteBatchAsync(It.IsAny<TableBatchOperation>(), null, null)).Verifiable();
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", ConnectionString = "Test Con", MailTemplateTableName = "MailTemplate" });
            var repo = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            await repo.CreateMeetingNotificationItemEntities(entities, this.applicationName);
            this.meetingHistoryTable.Verify(x => x.ExecuteBatchAsync(It.IsAny<TableBatchOperation>()), Times.Once);
        }

        /// <summary>
        /// Gets the meeting notification item entities tests.
        /// </summary>
        /// <returns>A Task.</returns>
        [Test]
        public async Task UpdateMeetingNotificationItemEntitiesTests()
        {
            this.meetingHistoryTable = new Mock<CloudTable>(new Uri("http://unittests.localhost.com/FakeTable"), (TableClientConfiguration)null);
            _ = this.cloudStorageClient.Setup(x => x.GetCloudTable("MeetingHistory")).Returns(this.meetingHistoryTable.Object);
            List<MeetingNotificationItemEntity> entities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1", Application = "Application", RowKey = "notificationId1", ETag = "*" }, new MeetingNotificationItemEntity { NotificationId = "notificationId2", Application = "Application", RowKey = "notificationId2", ETag = "*" } };
            this.meetingHistoryTable.Setup(x => x.ExecuteBatchAsync(It.IsAny<TableBatchOperation>(), null, null)).Verifiable();
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", ConnectionString = "Test Con", MailTemplateTableName = "MailTemplate" });
            var repo = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            await repo.UpdateMeetingNotificationItemEntities(entities);
            this.meetingHistoryTable.Verify(x => x.ExecuteBatchAsync(It.IsAny<TableBatchOperation>()), Times.Once);
        }
    }
}
