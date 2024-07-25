// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using Microsoft.Extensions.Options;
    using Moq;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models.Request;
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
        /// DateRange object.
        /// </summary>
        private readonly DateTimeRange dateRange = new DateTimeRange
        {
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddHours(2),
        };

        /// <summary>
        /// Instance of <see cref="meetingHistoryTable"/>.
        /// </summary>
        private Mock<TableClient> meetingHistoryTable;

        /// <summary>
        /// Instance of <see cref="meetingHistoryTable"/>.
        /// </summary>
        private Mock<TableServiceClient> tableServiceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageRepositoryTests"/> class.
        /// </summary>
        public TableStorageRepositoryTests()
        {

            this.cloudStorageClient = new Mock<ITableStorageClient>();
            this.tableServiceClient = new Mock<TableServiceClient>("http://unittests.localhost.com/FakeTable");
            this.logger = new Mock<ILogger>();
            this.mailAttachmentRepository = new Mock<IMailAttachmentRepository>();
            this.meetingHistoryTable = new Mock<TableClient>(new Uri("http://unittests.localhost.com/FakeTable"), (string)null);
        }

        /// <summary>
        /// Gets the meeting notification item entities tests.
        /// </summary>
        /// <returns>A Task.</returns>
        [Test]
        public async Task GetMeetingNotificationItemEntitiesTests()
        {
            List<MeetingNotificationItemTableEntity> entities = new List<MeetingNotificationItemTableEntity> { new MeetingNotificationItemTableEntity { NotificationId = "notificationId1" }, new MeetingNotificationItemTableEntity { NotificationId = "notificationId2" } };
            IList<MeetingNotificationItemEntity> itemEntities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1" }, new MeetingNotificationItemEntity { NotificationId = "notificationId1" } };
            this.mailAttachmentRepository.Setup(x => x.DownloadMeetingInvite(It.IsAny<IList<MeetingNotificationItemEntity>>(), It.IsAny<string>())).Returns(Task.FromResult(itemEntities));
            var meetingHistoryTable = new Mock<TableClient>(new Uri("http://unittests.localhost.com/FakeTable"), (string)null);
            _ = this.cloudStorageClient.Setup(x => x.GetRecordsAsync<MeetingNotificationItemTableEntity>("MeetingHistory", It.IsAny<string>())).Returns(Task.FromResult(entities));
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", StorageTableAccountURI = "Test Con", MailTemplateTableName = "MailTemplate", EmailHistoryTableName = "EmailHistory", MeetingHistoryTableName = "MeetingHistory", NotificationQueueName = "test-queue" });
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
            List<MeetingNotificationItemTableEntity> entities = new List<MeetingNotificationItemTableEntity> { new MeetingNotificationItemTableEntity { NotificationId = "notificationId1" } };
            IList<MeetingNotificationItemEntity> itemEntities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1" } };
            this.mailAttachmentRepository.Setup(x => x.DownloadMeetingInvite(It.IsAny<IList<MeetingNotificationItemEntity>>(), It.IsAny<string>())).Returns(Task.FromResult(itemEntities));
            var meetingHistoryTable = new Mock<TableClient>(new Uri("http://unittests.localhost.com/FakeTable"), (string)null);
            _ = this.cloudStorageClient.Setup(x => x.GetRecordsAsync<MeetingNotificationItemTableEntity>("MeetingHistory", It.IsAny<string>())).Returns(Task.FromResult(entities));
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", StorageTableAccountURI = "Test Con", MailTemplateTableName = "MailTemplate", EmailHistoryTableName = "EmailHistory", MeetingHistoryTableName = "MeetingHistory", NotificationQueueName = "test-queue" });
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
            IList<MeetingNotificationItemEntity> entities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1", Application = "Application", RowKey = "notificationId1" }, new MeetingNotificationItemEntity { NotificationId = "notificationId2", Application = "Application", RowKey = "notificationId2" } };
            _ = this.mailAttachmentRepository.Setup(e => e.UploadMeetingInvite(It.IsAny<IList<MeetingNotificationItemEntity>>(), It.IsAny<string>())).Returns(Task.FromResult(entities));
            _ = this.cloudStorageClient.Setup(x => x.InsertRecordsAsync<MeetingNotificationItemEntity>("MeetingHistory", entities.ToList())).Returns(Task.FromResult(true));
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", StorageTableAccountURI = "Test Con", MailTemplateTableName = "MailTemplate", EmailHistoryTableName = "EmailHistory", MeetingHistoryTableName = "MeetingHistory", NotificationQueueName = "test-queue" });
            var repo = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            var test = repo.CreateMeetingNotificationItemEntities(entities, this.applicationName).ConfigureAwait(true);
            Assert.IsNotNull(test);
        }

        /// <summary>
        /// Gets the meeting notification item entities tests.
        /// </summary>
        /// <returns>A Task.</returns>
        [Test]
        public async Task UpdateMeetingNotificationItemEntitiesTests()
        {
            this.meetingHistoryTable = new Mock<TableClient>(new Uri("http://unittests.localhost.com/FakeTable"), (string)null);  
            List<MeetingNotificationItemEntity> entities = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { NotificationId = "notificationId1", Application = "Application", RowKey = "notificationId1" }, new MeetingNotificationItemEntity { NotificationId = "notificationId2", Application = "Application", RowKey = "notificationId2" } };
            this.cloudStorageClient.Setup(x => x.AddsOrUpdatesAsync<MeetingNotificationItemEntity>("MeetingHistory", entities)).Verifiable();
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", StorageTableAccountURI = "Test Con", MailTemplateTableName = "MailTemplate", EmailHistoryTableName = "EmailHistory", MeetingHistoryTableName = "MeetingHistory", NotificationQueueName = "test-queue" });
            var repo = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            var test = repo.UpdateMeetingNotificationItemEntities(entities).ConfigureAwait(true);
            Assert.IsNotNull(test);
        }

        /// <summary>
        /// Tests for GetEmailNotificationsByDateRangeTest method for valid inputs.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetEmailNotificationItemEntitiesBetweenDatesTests()
        {
            var statusList = new List<NotificationItemStatus>() { NotificationItemStatus.Failed };
            var emailNotificationItemEntity = new EmailNotificationItemTableEntity()
            {
                Application = this.applicationName,
                NotificationId = Guid.NewGuid().ToString(),
            };
            var notificationList = new List<EmailNotificationItemTableEntity>() { emailNotificationItemEntity };
            var emailHistoryTable = new Mock<TableClient>(new Uri("http://unittests.localhost.com/FakeTable"), (string)null);
            _ = this.cloudStorageClient.Setup(x => x.GetRecordsAsync<EmailNotificationItemTableEntity>("EmailHistory", It.IsAny<string>())).Returns(Task.FromResult(notificationList));           
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", StorageTableAccountURI = "Test Con", MailTemplateTableName = "MailTemplate", EmailHistoryTableName = "EmailHistory", MeetingHistoryTableName = "MeetingHistory", NotificationQueueName = "test-queue" });
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
            _ = cloudStorageClient.Setup(x => x.GetRecordsAsync<EmailNotificationItemTableEntity>("EmailHistory", It.IsAny<string>())).Returns(Task.FromResult(notificationList));
            classUnderTest = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            result = await classUnderTest.GetPendingOrFailedEmailNotificationsByDateRange(this.dateRange, this.applicationName, null);
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests for GetMeetingNotificationsByDateRangeTest method for valid inputs.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetMeetingNotificationItemEntitiesBetweenDatesTests()
        {
            var statusList = new List<NotificationItemStatus>() { NotificationItemStatus.Failed };
            var meetingNotificationItemEntity = new MeetingNotificationItemTableEntity()
            {
                Application = this.applicationName,
                NotificationId = Guid.NewGuid().ToString(),
            };
            var notificationList = new List<MeetingNotificationItemTableEntity>() { meetingNotificationItemEntity };
            var meetingHistoryTable = new Mock<TableClient>(new Uri("http://unittests.localhost.com/FakeTable"), (string)null);
            _ = this.cloudStorageClient.Setup(x => x.GetRecordsAsync<MeetingNotificationItemTableEntity>("MeetingHistory", It.IsAny<string>())).Returns(Task.FromResult(notificationList));
            IOptions<StorageAccountSetting> options = Options.Create<StorageAccountSetting>(new StorageAccountSetting { BlobContainerName = "Test", StorageTableAccountURI = "Test Con", MailTemplateTableName = "MailTemplate", EmailHistoryTableName = "EmailHistory", MeetingHistoryTableName = "MeetingHistory", NotificationQueueName = "test-queue" });
            var classUnderTest = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            // dateRange is Null.
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await classUnderTest.GetPendingOrFailedMeetingNotificationsByDateRange(null, this.applicationName, statusList));
            // applicationname is Null
            var result = await classUnderTest.GetPendingOrFailedMeetingNotificationsByDateRange(this.dateRange, null, statusList);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.FirstOrDefault().NotificationId, meetingNotificationItemEntity.NotificationId);
            // NotificationStatusList is null
            result = await classUnderTest.GetPendingOrFailedMeetingNotificationsByDateRange(this.dateRange, this.applicationName, null);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.FirstOrDefault().NotificationId, meetingNotificationItemEntity.NotificationId);
            // Fetched records are null
            notificationList = null;
            _ = cloudStorageClient.Setup(x => x.GetRecordsAsync<MeetingNotificationItemTableEntity>("MeetingHistory", It.IsAny<string>())).Returns(Task.FromResult(notificationList));
            classUnderTest = new TableStorageEmailRepository(options, this.cloudStorageClient.Object, this.logger.Object, this.mailAttachmentRepository.Object);
            result = await classUnderTest.GetPendingOrFailedMeetingNotificationsByDateRange(this.dateRange, this.applicationName, null);
            Assert.IsNull(result);
        }
    }
}
