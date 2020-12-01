// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.V1.EmailManager
{    
    using Moq;
    using NotificationService.Data.Interfaces;
    using NotificationService.Common.Logger;
    using NotificationService.Common;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using NotificationService.Common.Encryption;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NUnit.Framework;
    using NotificationService.Contracts.Models;
    using System.Threading.Tasks;
    using NotificationService.Data;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts;

    /// <summary>
    /// EmailManagerTests.
    /// </summary>
    public class EmailManagerTests
    {
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

        public EmailManagerTests()
        {
            this.Logger = new Mock<ILogger>().Object;
            this.EncryptionService = new Mock<IEncryptionService>();
            this.TemplateManager = new Mock<IMailTemplateManager>();
            this.TemplateMerge = new Mock<ITemplateMerge>();
            this.Configuration = new Mock<IConfiguration>();
            this.EmailNotificationRepo = new Mock<IEmailNotificationRepository>();
            this.RepositoryFactory = new Mock<IRepositoryFactory>();
            _ = this.Configuration.Setup(x => x[Constants.StorageType]).Returns("StorageAccount");
            _ = this.RepositoryFactory.Setup(x => x.GetRepository(It.IsAny<StorageType>())).Returns(this.EmailNotificationRepo.Object);
        }

        /// <summary>
        /// Notifications the entities to response tests.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task CreateMeetingNotificationEntitiesTests()
        {
            var emailManager = new EmailManager(this.Configuration.Object, this.RepositoryFactory.Object, this.Logger, this.EncryptionService.Object, this.TemplateManager.Object, this.TemplateMerge.Object);
            var meetingNotificationItems = new List<MeetingNotificationItem> { new MeetingNotificationItem { EndDate = DateTime.UtcNow.AddHours(1), Start = DateTime.UtcNow.AddHours(1), End = DateTime.UtcNow },
                                                new MeetingNotificationItem { EndDate = DateTime.UtcNow.AddHours(1), Start = DateTime.UtcNow.AddHours(1), End = DateTime.UtcNow}, };
            var meetingEntities = await emailManager.CreateMeetingNotificationEntities("TestApp", meetingNotificationItems.ToArray(), NotificationService.Contracts.NotificationItemStatus.Queued);
            Assert.IsTrue(meetingEntities.Count == 2);
            this.EmailNotificationRepo.Verify(x => x.CreateMeetingNotificationItemEntities(It.IsAny<List<MeetingNotificationItemEntity>>()), Times.Once);
        }

        /// <summary>
        /// Notifications the entities to response tests.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public void NotificationEntitiesToResponseTests()
        {
            var emailManager = new EmailManager(this.Configuration.Object, this.RepositoryFactory.Object, this.Logger, this.EncryptionService.Object, this.TemplateManager.Object, this.TemplateMerge.Object);
            var meetingNotificationItems = new List<MeetingNotificationItemEntity> { new MeetingNotificationItemEntity { EndDate = DateTime.UtcNow.AddHours(1), Start = DateTime.UtcNow.AddHours(1), End = DateTime.UtcNow },
                                                new MeetingNotificationItemEntity { EndDate = DateTime.UtcNow.AddHours(1), Start = DateTime.UtcNow.AddHours(1), End = DateTime.UtcNow}, };
            var meetingEntities = emailManager.NotificationEntitiesToResponse(new List<NotificationResponse>(), meetingNotificationItems);
            Assert.IsTrue(meetingEntities.Count == 2);            
        }
    }
}
