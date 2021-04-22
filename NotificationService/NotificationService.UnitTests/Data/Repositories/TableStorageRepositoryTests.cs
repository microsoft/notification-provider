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
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models.GDPR;
    using NotificationService.Contracts.Models.Request;
    using NotificationService.Data;
    using NotificationService.Data.Repositories;
    using NUnit.Framework;
    using Org.BouncyCastle.Math.EC.Rfc7748;

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
        /// Instace of <see cref="CloudTable"/>
        /// </summary>
        private readonly Mock<CloudTable> cloudTable;

        /// <summary>
        /// storageconfiguration options.
        /// </summary>
        IOptions<StorageAccountSetting> storageConfigOptions;

        /// <summary>
        /// DateRange object.
        /// </summary>
        private readonly DateTimeRange dateRange = new DateTimeRange
        {
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddHours(2),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageRepositoryTests"/> class.
        /// </summary>
        public TableStorageRepositoryTests()
        {
            this.cloudStorageClient = new Mock<ITableStorageClient>();
            this.logger = new Mock<ILogger>();
            this.mailAttachmentRepository = new Mock<IMailAttachmentRepository>();
            this.cloudTable = new Mock<CloudTable>(new Uri("https://test.azurestorage.com/testtable"), (TableClientConfiguration) null);
            this.meetingHistoryTable = new Mock<CloudTable>(new Uri("http://unittests.localhost.com/FakeTable"), (TableClientConfiguration)null);
            this.storageConfigOptions = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", ConnectionString = "Test Con", MailTemplateTableName = "MailTemplate", EmailHistoryTableName = "EmailHistory", MeetingHistoryTableName = "MeetingHistory", NotificationQueueName = "test-queue", EmailNotificationMapTableName = "EmailMappingTable", MeetingNotificationMapTableName = "MeetingMappingTable" });
            _ = this.cloudStorageClient.Setup(x => x.GetCloudTable("MeetingHistory")).Returns(this.meetingHistoryTable.Object);
            _ = this.cloudStorageClient.Setup(x => x.GetCloudTable(It.IsAny<string>())).Returns(this.cloudTable.Object);
            TableResult res = new TableResult() { HttpStatusCode = 200};
            _ = this.cloudTable.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(res);
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
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", ConnectionString = "Test Con", MailTemplateTableName = "MailTemplate", EmailHistoryTableName = "EmailHistory", MeetingHistoryTableName = "MeetingHistory", NotificationQueueName = "test-queue" });
            var repo = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            var items = await repo.GetMeetingNotificationItemEntities(new List<string> { "notificationId1", "notificationId2" }, this.applicationName);
            Assert.IsTrue(items.Count == 2);
        }

        /// <summary>
        /// Gets the meeting notification item entities tests.
        /// </summary>
        /// <returns>A Task.</returns>
        [Test]
        public async Task GetMeetingNotificationItemEntityTests()
        {
            IEnumerable<MeetingNotificationItemTableEntity> entities = new List<MeetingNotificationItemTableEntity> { new MeetingNotificationItemTableEntity { NotificationId = "notificationId1" } };
            IList<MeetingNotificationItemEntity> itemEntities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1" } };
            _ = this.mailAttachmentRepository.Setup(x => x.DownloadMeetingInvite(It.IsAny<IList<MeetingNotificationItemEntity>>(), It.IsAny<string>())).Returns(Task.FromResult(itemEntities));
            _ = this.meetingHistoryTable.Setup(x => x.ExecuteQuery(It.IsAny<TableQuery<MeetingNotificationItemTableEntity>>(), null, null)).Returns(entities);
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", ConnectionString = "Test Con", MailTemplateTableName = "MailTemplate", EmailHistoryTableName = "EmailHistory", MeetingHistoryTableName = "MeetingHistory", NotificationQueueName = "test-queue" });
            var repo = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            var items = await repo.GetMeetingNotificationItemEntity("notificationId1", this.applicationName);
            Assert.IsTrue(items.NotificationId == "notificationId1");
        }

        /// <summary>
        /// Gets the meeting notification item entities tests.
        /// </summary>
        /// <returns>A Task.</returns>
        [Test]
        public async Task CreateMeetingNotificationItemEntitiesTests()
        {
            this.meetingHistoryTable = new Mock<CloudTable>(new Uri("http://unittests.localhost.com/FakeTable"), (TableClientConfiguration)null);
            _ = this.cloudStorageClient.Setup(x => x.GetCloudTable("MeetingHistory")).Returns(this.meetingHistoryTable.Object);
            IList<MeetingNotificationItemEntity> entities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1", Application = "Application", RowKey = "notificationId1" }, new MeetingNotificationItemEntity { NotificationId = "notificationId2", Application = "Application", RowKey = "notificationId2" } };
            _ = this.mailAttachmentRepository.Setup(e => e.UploadMeetingInvite(It.IsAny<IList<MeetingNotificationItemEntity>>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(entities));
            this.meetingHistoryTable.Setup(x => x.ExecuteBatchAsync(It.IsAny<TableBatchOperation>())).Verifiable();
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", ConnectionString = "Test Con", MailTemplateTableName = "MailTemplate", EmailHistoryTableName = "EmailHistory", MeetingHistoryTableName = "MeetingHistory", NotificationQueueName = "test-queue" });
            var repo = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            await repo.CreateMeetingNotificationItemEntities(entities, this.applicationName);
            this.meetingHistoryTable.Verify(x => x.ExecuteBatchAsync(It.Is<TableBatchOperation>(x => x.Any(y => y.OperationType == TableOperationType.Insert))), Times.Once);
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
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", ConnectionString = "Test Con", MailTemplateTableName = "MailTemplate", EmailHistoryTableName = "EmailHistory", MeetingHistoryTableName = "MeetingHistory", NotificationQueueName = "test-queue" });
            var repo = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            await repo.UpdateMeetingNotificationItemEntities(entities);
            this.meetingHistoryTable.Verify(x => x.ExecuteBatchAsync(It.Is<TableBatchOperation>(x => x.Any(y => y.OperationType == TableOperationType.Merge))), Times.Once);
        }

        /// <summary>
        /// Tests for GetEmailNotificationsByDateRangeTest method for valid inputs.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetEmailNotificationItemEntitiesBetweenDatesTests()
        {
            var statusList = new List<NotificationItemStatus>() { NotificationItemStatus.Failed};
            var emailNotificationItemEntity = new EmailNotificationItemTableEntity()
            {
                Application = this.applicationName,
                NotificationId = Guid.NewGuid().ToString(),
            };
            var notificationList = new List<EmailNotificationItemTableEntity>() { emailNotificationItemEntity };
            var emailHistoryTable = new Mock<CloudTable>(new Uri("http://unittests.localhost.com/FakeTable"), (TableClientConfiguration)null);
            _ = this.cloudStorageClient.Setup(x => x.GetCloudTable("EmailHistory")).Returns(emailHistoryTable.Object);
            _ = emailHistoryTable.Setup(x => x.ExecuteQuery(It.IsAny<TableQuery<EmailNotificationItemTableEntity>>(), It.IsAny<TableRequestOptions>(), It.IsAny<OperationContext>())).Returns(notificationList);
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", ConnectionString = "Test Con", MailTemplateTableName = "MailTemplate", EmailHistoryTableName = "EmailHistory", MeetingHistoryTableName = "MeetingHistory", NotificationQueueName = "test-queue" });
            var classUnderTest = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);

            // dateRange is Null.
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await classUnderTest.GetPendingOrFailedEmailNotificationsByDateRange(null, this.applicationName, statusList));

            // applicationname is Null
            var result = await classUnderTest.GetPendingOrFailedEmailNotificationsByDateRange(this.dateRange, null, statusList);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.FirstOrDefault().NotificationId, emailNotificationItemEntity.NotificationId);

            // NotificationStatusList is null
            result = await classUnderTest.GetPendingOrFailedEmailNotificationsByDateRange(this.dateRange, this.applicationName, null);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.FirstOrDefault().NotificationId, emailNotificationItemEntity.NotificationId);

            // Fetched records are null
            notificationList = null;
            _ = emailHistoryTable.Setup(x => x.ExecuteQuery(It.IsAny<TableQuery<EmailNotificationItemTableEntity>>(), It.IsAny<TableRequestOptions>(), It.IsAny<OperationContext>())).Returns(notificationList);
            classUnderTest = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            result = await classUnderTest.GetPendingOrFailedEmailNotificationsByDateRange(this.dateRange, this.applicationName, null);
            Assert.IsNull(result);
        }

        /// <summary>
        /// Create NotificationId and EmailId mapping for email Notifications. Test for Success.
        /// </summary>
        [Test]
        public void CreateEmailIdNotificationMappingForEmail_Success()
        {
            this.storageConfigOptions.Value.EmailNotificationMapTableName = "EmailNotificationMapTable";
            var item = new List<EmailNotificationQueueItem>() { new EmailNotificationQueueItem() { To = "test1@contoso.com;test2@contoso.com", CC = "tst@abc.com", From = "abc@contosot.com", NotificationId = Guid.NewGuid().ToString() } };
            var classUnderTest = new TableStorageEmailRepository(this.storageConfigOptions, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            classUnderTest.CreateEmailIdNotificationMappingForEmail(item, this.applicationName);
            this.cloudStorageClient.Verify(x => x.GetCloudTable(It.IsAny<string>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Create NotificationId and EmailId mapping for email Notifications. Test for Failures.
        /// </summary>
        [Test]
        public void CreateEmailIdNotificationMappingForEmail_Failed()
        {
            var item = new List<EmailNotificationQueueItem>() { new EmailNotificationQueueItem() { To = "test1@contoso.com;test2@contoso.com", CC = "tst@abc.com", From = "abc@contosot.com", NotificationId = Guid.NewGuid().ToString() } };
            var classUnderTest = new TableStorageEmailRepository(this.storageConfigOptions, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);

            // Test for null EmailNotificationQueueItem list.
            classUnderTest.CreateEmailIdNotificationMappingForEmail(null, this.applicationName);

            this.storageConfigOptions.Value.EmailNotificationMapTableName = null;
            classUnderTest = new TableStorageEmailRepository(this.storageConfigOptions, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);

            // Test for Table does not exists exception.
            try
            {
                classUnderTest.CreateEmailIdNotificationMappingForEmail(item, this.applicationName);
            } catch(Exception ex)
            {
                Assert.IsTrue(ex is ArgumentNullException);
            }
        }

        /// <summary>
        /// Create NotificationId and EmailId mapping for meeting Notifications. Test For Success.
        /// </summary>
        [Test]
        public void CreateEmailIdNotificationForMeetingInvitesMapping_Success()
        {
            this.storageConfigOptions.Value.MeetingNotificationMapTableName = "MeetingNotificationMapTable";
            var item = new List<MeetingNotificationQueueItem>() { new MeetingNotificationQueueItem() { RequiredAttendees = "test1@contoso.com;test2@contoso.com", OptionalAttendees = "tst@abc.com", From = "abc@contosot.com", NotificationId = Guid.NewGuid().ToString() } };
            var classUnderTest = new TableStorageEmailRepository(this.storageConfigOptions, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            classUnderTest.CreateEmailIdNotificationMappingForMeetingInvite(item, this.applicationName);
            this.cloudStorageClient.Verify(x => x.GetCloudTable(It.IsAny<string>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Create NotificationId and EmailId mapping for meeting Notifications. Test For Failure.
        /// </summary>
        [Test]
        public void CreateEmailIdNotificationForMeetingInvitesMapping_Failed()
        {
            var item = new List<MeetingNotificationQueueItem>() { new MeetingNotificationQueueItem() { RequiredAttendees = "test1@contoso.com;test2@contoso.com", OptionalAttendees = "tst@abc.com", From = "abc@contosot.com", NotificationId = Guid.NewGuid().ToString() } };
            var classUnderTest = new TableStorageEmailRepository(this.storageConfigOptions, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);

            // Test for null EmailNotificationQueueItem list.
            classUnderTest.CreateEmailIdNotificationMappingForMeetingInvite(null, this.applicationName);

            this.storageConfigOptions.Value.MeetingNotificationMapTableName = null;
            classUnderTest = new TableStorageEmailRepository(this.storageConfigOptions, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);

            // Test for Table does not exists exception.
            try
            {
                classUnderTest.CreateEmailIdNotificationMappingForMeetingInvite(item, this.applicationName);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentNullException);
            }
        }
    }
}
