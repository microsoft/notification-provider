// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.V1.EmailManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
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
        /// Initializes a new instance of the <see cref="EmailManagerTests"/> class.
        /// </summary>
        public EmailManagerTests()
        {
            this.Logger = new Mock<ILogger>().Object;
            this.EncryptionService = new Mock<IEncryptionService>();
            this.TemplateManager = new Mock<IMailTemplateManager>();
            this.TemplateMerge = new Mock<ITemplateMerge>();
            this.Configuration = new Mock<IConfiguration>();
            this.EmailNotificationRepo = new Mock<IEmailNotificationRepository>();
            this.CloudStorageAccount = new Mock<ICloudStorageClient>();
            this.RepositoryFactory = new Mock<IRepositoryFactory>();
            _ = this.Configuration.Setup(x => x[ConfigConstants.StorageType]).Returns("StorageAccount");
            _ = this.Configuration.Setup(x => x[ConfigConstants.IsGDPREnabled]).Returns("false");
            _ = this.RepositoryFactory.Setup(x => x.GetRepository(It.IsAny<StorageType>())).Returns(this.EmailNotificationRepo.Object);
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
        public Mock<IConfiguration> Configuration { get; set; }

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
        public Mock<ICloudStorageClient> CloudStorageAccount { get; set; }

        /// <summary>
        /// Notifications the entities to response tests.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task CreateMeetingNotificationEntitiesTests()
        {
            var emailManager = new EmailManager(this.Configuration.Object, this.RepositoryFactory.Object, this.Logger, this.TemplateManager.Object, this.TemplateMerge.Object, this.CloudStorageAccount.Object);
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
            var emailManager = new EmailManager(this.Configuration.Object, this.RepositoryFactory.Object, this.Logger, this.TemplateManager.Object, this.TemplateMerge.Object, this.CloudStorageAccount.Object);
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
    }
}
