// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.V1.EmailManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Storage.Queue;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Encryption;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models;
    using NotificationService.Data;
    using NotificationService.Data.Interfaces;
    using NUnit.Framework;

    /// <summary>
    /// EmailManagerTests.
    /// </summary>
    public class EmailManagerTests
    {
        /// <summary>
        /// Application Name Constant.
        /// </summary>
        public const string ApplicationName = "TestApp";

        /// <summary>
        /// CloudQueue instance ref.
        /// </summary>
        private readonly Mock<CloudQueue> cloudQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailManagerTests"/> class.
        /// </summary>
        public EmailManagerTests()
        {
            this.Logger = new Mock<ILogger>().Object;
            this.EncryptionService = new Mock<IEncryptionService>();
            this.TemplateManager = new Mock<IMailTemplateManager>();
            this.TemplateMerge = new Mock<ITemplateMerge>();
            this.EmailNotificationRepo = new Mock<IEmailNotificationRepository>();
            this.CloudStorageClient = new Mock<ICloudStorageClient>();
            this.RepositoryFactory = new Mock<IRepositoryFactory>();
            this.cloudQueue = new Mock<CloudQueue>(new Uri("https://test.azure.com/testqueue"));

            var testConfigValues = new Dictionary<string, string>()
            {
                { ConfigConstants.StorageType, "StorageAccount" },
                { ConfigConstants.IsGDPREnabled, "false" },
                { ConfigConstants.StorageAccGdprMapQueueName,  "TestQueue" },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            _ = this.RepositoryFactory.Setup(x => x.GetRepository(It.IsAny<StorageType>())).Returns(this.EmailNotificationRepo.Object);
            _ = this.CloudStorageClient.Setup(x => x.GetCloudQueue(It.IsAny<string>())).Returns(this.cloudQueue.Object);
            this.CloudStorageClient.Setup(x => x.QueueCloudMessages(It.IsAny<CloudQueue>(), It.IsAny<List<string>>(), It.IsAny<TimeSpan>())).Verifiable();
        }

        /// <summary>
        /// Gets or sets Email Notification Repository Mock.
        /// </summary>
        public Mock<IRepositoryFactory> EmailNotificationRepository { get; set; }

        /// <summary>
        /// Gets or sets Logger.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets Configuration.
        /// </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets Encryption Service Mock.
        /// </summary>
        public Mock<IEncryptionService> EncryptionService { get; set; }

        /// <summary>
        /// Gets or sets TemplateManager Mock.
        /// </summary>
        public Mock<IMailTemplateManager> TemplateManager { get; set; }

        /// <summary>
        /// Gets or sets TemplateMerge Mock.
        /// </summary>
        public Mock<ITemplateMerge> TemplateMerge { get; set; }

        /// <summary>
        /// Gets or sets the repository factory.
        /// </summary>
        /// <value>
        /// The repository factory.
        /// </value>
        public Mock<IRepositoryFactory> RepositoryFactory { get; set; }

        /// <summary>
        /// Gets or sets the email notification repository.
        /// </summary>
        /// <value>
        /// The email notification repository.
        /// </value>
        public Mock<IEmailNotificationRepository> EmailNotificationRepo { get; set; }

        /// <summary>
        /// Gets or sets cloutd Storage account mocked instance.
        /// </summary>
        public Mock<ICloudStorageClient> CloudStorageClient { get; set; }

        /// <summary>
        /// Notifications the entities to response tests.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task CreateMeetingNotificationEntitiesTests()
        {
            var emailManager = new EmailManager(this.Configuration, this.RepositoryFactory.Object, this.Logger, this.TemplateManager.Object, this.TemplateMerge.Object, this.CloudStorageClient.Object);
            var meetingNotificationItems = new List<MeetingNotificationItem>
            {
                new MeetingNotificationItem { EndDate = DateTime.UtcNow.AddHours(1), Start = DateTime.UtcNow.AddHours(1), End = DateTime.UtcNow },
                new MeetingNotificationItem { EndDate = DateTime.UtcNow.AddHours(1), Start = DateTime.UtcNow.AddHours(1), End = DateTime.UtcNow },
            };
            var meetingEntities = await emailManager.CreateMeetingNotificationEntities("TestApp", meetingNotificationItems.ToArray(), NotificationService.Contracts.NotificationItemStatus.Queued);
            Assert.IsTrue(meetingEntities.Count == 2);
            this.EmailNotificationRepo.Verify(x => x.CreateMeetingNotificationItemEntities(It.IsAny<List<MeetingNotificationItemEntity>>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Notifications the entities to response tests.
        /// </summary>
        [Test]
        public void NotificationEntitiesToResponseTests()
        {
            var emailManager = new EmailManager(this.Configuration, this.RepositoryFactory.Object, this.Logger, this.TemplateManager.Object, this.TemplateMerge.Object, this.CloudStorageClient.Object);
            var meetingNotificationItems = new List<MeetingNotificationItemEntity>
            {
                new MeetingNotificationItemEntity
                {
                    EndDate = DateTime.UtcNow.AddHours(1),
                    Start = DateTime.UtcNow.AddHours(1),
                    End = DateTime.UtcNow,
                },
                new MeetingNotificationItemEntity { EndDate = DateTime.UtcNow.AddHours(1), Start = DateTime.UtcNow.AddHours(1), End = DateTime.UtcNow },
            };
            var meetingEntities = emailManager.NotificationEntitiesToResponse(new List<NotificationResponse>(), meetingNotificationItems);
            Assert.IsTrue(meetingEntities.Count == 2);
        }

        /// <summary>
        /// Queue EmailNotification for GDPR Mapping.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task QueueEmailNotificaitionGDPRMappingTest_Success()
        {
            var emailNotificationItemEntities = new List<List<EmailNotificationItemEntity>>()
            {
                new List<EmailNotificationItemEntity>()
                {
                    new EmailNotificationItemEntity()
                    {
                        Application = ApplicationName,
                        To = "test@abc.com",
                        From = "abc@contoso.com",
                        NotificationId = "1234",
                        Body = "Test Email",
                    },
                },
            };
            var testConfigValues = new Dictionary<string, string>()
            {
                { ConfigConstants.StorageType, "StorageAccount" },
                { ConfigConstants.IsGDPREnabled, "true" },
                { ConfigConstants.StorageAccGdprMapQueueName,  "TestQueue" },
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();
            var emailManager = new EmailManager(config, this.RepositoryFactory.Object, this.Logger, this.TemplateManager.Object, this.TemplateMerge.Object, this.CloudStorageClient.Object);
            await emailManager.QueueEmailNotificaitionMapping(ApplicationName, emailNotificationItemEntities, null);
            this.CloudStorageClient.Verify(x => x.GetCloudQueue(It.IsAny<string>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Queue Meeting Notification for GDPR Mapping.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task QueueMeetingNotificaitionGDPRMappingTest_Success()
        {
            var meetingNotificationItemEntities = new List<List<MeetingNotificationItemEntity>>()
            {
                new List<MeetingNotificationItemEntity>()
                {
                    new MeetingNotificationItemEntity()
                    {
                        Application = ApplicationName,
                        RequiredAttendees = "test@abc.com",
                        From = "abc@contoso.com",
                        NotificationId = "1234",
                        Body = "Test Email",
                    },
                },
            };
            var testConfigValues = new Dictionary<string, string>()
            {
                { ConfigConstants.StorageType, "StorageAccount" },
                { ConfigConstants.IsGDPREnabled, "true" },
                { ConfigConstants.StorageAccGdprMapQueueName,  "TestQueue" },
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();
            var emailManager = new EmailManager(config, this.RepositoryFactory.Object, this.Logger, this.TemplateManager.Object, this.TemplateMerge.Object, this.CloudStorageClient.Object);
            await emailManager.QueueMeetingNotificactionMapping(ApplicationName, meetingNotificationItemEntities, null);
            this.CloudStorageClient.Verify(x => x.GetCloudQueue(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
