// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Controllers.V1.EmailController
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Storage.Queue;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Moq;
    using Newtonsoft.Json;
    using NotificationHandler.Controllers;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Business.V1;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models.Request;
    using NotificationService.Data;
    using NUnit.Framework;

    /// <summary>
    /// Tests for ResendEmailNotifications method of Email Controller.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ResendEmailNotificationsRequestHandlerTests
    {
        private readonly string applicationName = "TestApp";
        private readonly string[] notificationIds = new string[]
        {
                Guid.NewGuid().ToString(),
        };

        private readonly DateTimeRange dateRange = new DateTimeRange()
        {
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddHours(2),
        };

        private Mock<IEmailHandlerManager> emailHandlerManager;
        private Mock<IMailTemplateManager> mailTemplateManager;
        private Mock<ICloudStorageClient> cloudStorageClient;
        private Mock<IEmailManager> emailManager;
        private Mock<ILogger> logger;
        private CloudQueue cloudQueue;
        private IConfiguration configuration;
        private Mock<IOptions<MSGraphSetting>> msGraphSettingOptions;

        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.emailHandlerManager = new Mock<IEmailHandlerManager>();
            this.mailTemplateManager = new Mock<IMailTemplateManager>();
            this.logger = new Mock<ILogger>();
            this.cloudStorageClient = new Mock<ICloudStorageClient>();
            this.emailManager = new Mock<IEmailManager>();
            this.msGraphSettingOptions = new Mock<IOptions<MSGraphSetting>>();
            var config = new Dictionary<string, string>()
            {
                { ConfigConstants.AllowedMaxResendDurationInDays, "1" },
                { ConfigConstants.StorageAccountConfigSectionKey, JsonConvert.SerializeObject(new StorageAccountSetting() { NotificationQueueName = "test-queue", }) },
            };

            this.configuration = new ConfigurationBuilder().AddInMemoryCollection(config).Build();
            this.cloudQueue = new CloudQueue(new Uri("https://test.com/endpoint"));
            _ = this.cloudStorageClient.Setup(x => x.GetCloudQueue(It.IsAny<string>())).Returns(this.cloudQueue);
            _ = this.cloudStorageClient.Setup(x => x.QueueCloudMessages(It.IsAny<CloudQueue>(), It.IsAny<IEnumerable<string>>(), It.IsAny<TimeSpan>()));
            _ = this.logger.Setup(x => x.TraceInformation(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()));
        }

        /// <summary>
        /// Tests for ResendEmailNotifications method for valid inputs.
        /// </summary>
        [Test]
        public void ResendEmailNotificationsTestValidInput()
        {
            EmailController emailController = new EmailController(this.emailHandlerManager.Object, this.mailTemplateManager.Object, this.logger.Object);
            IList<NotificationResponse> responses = new List<NotificationResponse>();

            _ = this.emailHandlerManager
                .Setup(emailHandlerManager => emailHandlerManager.ResendNotifications(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(responses));

            var result = emailController.ResendEmailNotifications(this.applicationName, this.notificationIds);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.emailHandlerManager.Verify(mgr => mgr.ResendNotifications(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<NotificationType>(), It.IsAny<bool>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for ResendEmailNotifications method for invalid inputs.
        /// </summary>
        [Test]
        public void ResendEmailNotificationsTestInvalidInput()
        {
            EmailController emailController = new EmailController(this.emailHandlerManager.Object, this.mailTemplateManager.Object, this.logger.Object);

            _ = Assert.ThrowsAsync<ArgumentException>(async () => await emailController.ResendEmailNotifications(null, this.notificationIds));
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await emailController.ResendEmailNotifications(this.applicationName, null));
        }

        /// <summary>
        /// Tests for ResendEmailNotificationsByDateRange method for valid inputs.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ResendEmailNotificatoinByDateRangeTest_ValidInput()
        {
            var notificationItems = this.GetEmailNotificationItemEntities();
            _ = this.emailManager.Setup(x => x.GetEmailNotificationsByDateRangeAndStatus(It.IsAny<string>(), It.IsAny<DateTimeRange>(), It.IsAny<List<NotificationItemStatus>>())).ReturnsAsync(notificationItems);
            var classUnderTest = new EmailHandlerManager(this.configuration, this.msGraphSettingOptions.Object, this.cloudStorageClient.Object, this.logger.Object, this.emailManager.Object);
            var result = await classUnderTest.ResendEmailNotificationsByDateRange(this.applicationName, this.dateRange);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result.FirstOrDefault().NotificationId, notificationItems.FirstOrDefault().NotificationId);
            Assert.AreEqual(result.FirstOrDefault().Status, NotificationItemStatus.Queued);
        }

        /// <summary>
        /// Tests for ResendEmailNotificationsByDateRange method for valid inputs but with no records found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ResendEmailNotificatoinByDateRangeTest_ValidInput_NoRecordFound()
        {
            IList<EmailNotificationItemEntity> notificationItems = null;
            _ = this.emailManager.Setup(x => x.GetEmailNotificationsByDateRangeAndStatus(It.IsAny<string>(), It.IsAny<DateTimeRange>(), It.IsAny<List<NotificationItemStatus>>())).ReturnsAsync(notificationItems);
            var classUnderTest = new EmailHandlerManager(this.configuration, this.msGraphSettingOptions.Object, this.cloudStorageClient.Object, this.logger.Object, this.emailManager.Object);
            var result = await classUnderTest.ResendEmailNotificationsByDateRange(this.applicationName, this.dateRange);
            Assert.IsNull(result);

            notificationItems = new List<EmailNotificationItemEntity>();
            _ = this.emailManager.Setup(x => x.GetEmailNotificationsByDateRangeAndStatus(It.IsAny<string>(), It.IsAny<DateTimeRange>(), It.IsAny<List<NotificationItemStatus>>())).ReturnsAsync(notificationItems);
            classUnderTest = new EmailHandlerManager(this.configuration, this.msGraphSettingOptions.Object, this.cloudStorageClient.Object, this.logger.Object, this.emailManager.Object);
            result = await classUnderTest.ResendEmailNotificationsByDateRange(this.applicationName, this.dateRange);
            Assert.IsNull(result);
        }

        private IList<EmailNotificationItemEntity> GetEmailNotificationItemEntities()
        {
            IList<EmailNotificationItemEntity> list = new List<EmailNotificationItemEntity>()
            {
                new EmailNotificationItemEntity()
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    Application = this.applicationName,
                    Subject = "Test",
                    Body = "Test Content",
                    From = "test@abc.com",
                    To = "test1@abc.com",
                },
            };
            return list;
        }
    }
}
