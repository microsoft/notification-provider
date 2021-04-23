// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Controllers.V1.EmailController
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NotificationHandler.Controllers;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models.Request;
    using NUnit.Framework;

    /// <summary>
    /// Tests for ResendEmailNotifications method of Email Controller.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ResendEmailNotificationsTests
    {
        private readonly string applicationName = "TestApp";
        private readonly string[] notificationIds = new string[]
        {
                Guid.NewGuid().ToString(),
        };

        private Mock<IEmailHandlerManager> emailHandlerManager;
        private Mock<IMailTemplateManager> mailTemplateManager;
        private ILogger logger;

        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.emailHandlerManager = new Mock<IEmailHandlerManager>();
            this.mailTemplateManager = new Mock<IMailTemplateManager>();
            this.logger = new Mock<ILogger>().Object;
        }

        /// <summary>
        /// Tests for ResendEmailNotifications method for valid inputs.
        /// </summary>
        [Test]
        public void ResendEmailNotificationsTestValidInput()
        {
            EmailController emailController = new EmailController(this.emailHandlerManager.Object, this.mailTemplateManager.Object, this.logger);
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
            EmailController emailController = new EmailController(this.emailHandlerManager.Object, this.mailTemplateManager.Object, this.logger);

            _ = Assert.ThrowsAsync<ArgumentException>(async () => await emailController.ResendEmailNotifications(null, this.notificationIds));
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await emailController.ResendEmailNotifications(this.applicationName, null));
        }

        /// <summary>
        /// Tests for ResendEMailNotifications By Date Range.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ResendEmailNotificationsByDateRangeTest_ValidInput()
        {
            EmailController emailController = new EmailController(this.emailHandlerManager.Object, this.mailTemplateManager.Object, this.logger);
            IList<NotificationResponse> responses = new List<NotificationResponse>() { new NotificationResponse() { NotificationId = Guid.NewGuid().ToString(), Status = NotificationItemStatus.Queued, } };
            var dateRange = new DateTimeRange()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(4),
            };

            _ = this.emailHandlerManager
                .Setup(emailHandlerManager => emailHandlerManager.ResendEmailNotificationsByDateRange(It.IsAny<string>(), It.IsAny<DateTimeRange>()))
                .Returns(Task.FromResult(responses));

            var result = await emailController.ResendEmailNotificationsByDateRange(this.applicationName, dateRange);
            var res = (AcceptedResult)result;
            Assert.IsNotNull(res.Value);
            Assert.IsTrue(((List<NotificationResponse>)res.Value).Count == 1);
        }

        /// <summary>
        /// Tests for ResendEMailNotifications By Date Range.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ResendEmailNotificationsByDateRangeTest_ValidInput_NoRecordToProcess()
        {
            EmailController emailController = new EmailController(this.emailHandlerManager.Object, this.mailTemplateManager.Object, this.logger);
            IList<NotificationResponse> responses = new List<NotificationResponse>();
            var dateRange = new DateTimeRange()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(4),
            };

            _ = this.emailHandlerManager
                .Setup(emailHandlerManager => emailHandlerManager.ResendEmailNotificationsByDateRange(It.IsAny<string>(), It.IsAny<DateTimeRange>()))
                .Returns(Task.FromResult(responses));

            var result = await emailController.ResendEmailNotificationsByDateRange(this.applicationName, dateRange);
            var res = (AcceptedResult)result;
            Assert.IsNotNull(res.Value);
            responses = null;
            _ = this.emailHandlerManager
               .Setup(emailHandlerManager => emailHandlerManager.ResendEmailNotificationsByDateRange(It.IsAny<string>(), It.IsAny<DateTimeRange>()))
               .Returns(Task.FromResult(responses));
            result = await emailController.ResendEmailNotificationsByDateRange(this.applicationName, dateRange);
            res = (AcceptedResult)result;
            Assert.IsNull(res.Value);
        }

        /// <summary>
        /// Tests for ResendEMailNotifications By Date Range with invalid input.
        /// </summary>
        [Test]
        public void ResendEmailNotificationsByDateRangeTest_InvalidInput()
        {
            EmailController emailController = new EmailController(this.emailHandlerManager.Object, this.mailTemplateManager.Object, this.logger);
            _ = Assert.ThrowsAsync<ArgumentException>(async () => await emailController.ResendEmailNotificationsByDateRange(null, null));
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await emailController.ResendEmailNotificationsByDateRange(this.applicationName, null));
        }
    }
}
