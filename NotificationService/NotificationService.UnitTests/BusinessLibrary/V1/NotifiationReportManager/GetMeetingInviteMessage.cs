// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.V1.NotifiationReportManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NotificationService.BusinessLibrary;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Data;
    using NUnit.Framework;

    /// <summary>
    /// GetMettingInviteMessage Test.
    /// </summary>
    public class GetMeetingInviteMessage : NotificationReportManagerTestBase
    {
        /// <summary>
        /// Setup.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.SetupBase();
            var testConfig = new Dictionary<string, string>
            {
                { "StorageType", "StorageAccount" },
            };

            this.configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfig)
                .Build();
        }

        /// <summary>
        /// Test GetMeetingInviteMessage Success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetMeetingInviteMessageTest_Success()
        {
            string notifiationId = Guid.NewGuid().ToString();
            var meetingItemEntity = this.GetMeetingNotificationItemEntity();
            var emailRepository = new Mock<IEmailNotificationRepository>();
            _ = this.repositoryFactory.Setup(c => c.GetRepository(It.IsAny<StorageType>())).Returns(emailRepository.Object);
            _ = emailRepository.Setup(c => c.GetMeetingNotificationItemEntity(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(meetingItemEntity);
            _ = this.templateManager.Setup(c => c.GetMailTemplate(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(this.GetMailTemplate());
            _ = this.templateMerge.Setup(c => c.CreateMailBodyUsingTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("Test Message Body.");
            this.classUnderTest = new NotificationReportManager(this.logger.Object, this.repositoryFactory.Object, this.configuration, this.mailTemplateRepository.Object, this.templateManager.Object, this.templateMerge.Object);
            var result = await this.classUnderTest.GetMeetingNotificationMessage(this.ApplicationName, notifiationId);
            emailRepository.Verify(c => c.GetMeetingNotificationItemEntity(notifiationId, this.ApplicationName), Times.Once);
            Assert.AreEqual(meetingItemEntity.NotificationId, result.NotificationId);
        }

        /// <summary>
        /// Test GetMeetingInviteMessage No Entity Found for given notificationId.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task GetMeetingInviteMessageTest_EntityNotFoundException()
        {
            string notifiationId = Guid.NewGuid().ToString();
            var meetingItemEntity = this.GetMeetingNotificationItemEntity();
            var emailRepository = new Mock<IEmailNotificationRepository>();
            _ = this.repositoryFactory.Setup(c => c.GetRepository(It.IsAny<StorageType>())).Returns(emailRepository.Object);
            _ = emailRepository.Setup(c => c.GetMeetingNotificationItemEntity(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((MeetingNotificationItemEntity)null);
            _ = this.templateMerge.Setup(c => c.CreateMailBodyUsingTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("Test Message Body.");
            this.classUnderTest = new NotificationReportManager(this.logger.Object, this.repositoryFactory.Object, this.configuration, this.mailTemplateRepository.Object, this.templateManager.Object, this.templateMerge.Object);
            try
            {
                var result = await this.classUnderTest.GetMeetingNotificationMessage(this.ApplicationName, notifiationId);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentNullException);
            }
        }

        private MailTemplate GetMailTemplate()
        {
            return new MailTemplate()
            {
                Content = "Sample1: __var1__, Sample2: __var2__.",
                Description = "descr",
                TemplateId = "testTemplate",
                TemplateType = "HTML",
            };
        }

        private MeetingNotificationItemEntity GetMeetingNotificationItemEntity()
        {
            return new MeetingNotificationItemEntity()
            {
                NotificationId = Guid.NewGuid().ToString(),
                Application = this.ApplicationName,
                TemplateData = "{\"__var1__\":\"value1\", \"__var2__\":\"value2\"}",
                TemplateId = "testTemplate",
                From = "test@abc.com",
            };
        }
    }
}
