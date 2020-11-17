// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Controllers.V1.EmailController
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Moq;
    using NotificationHandler.Controllers;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
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
                .Setup(emailHandlerManager => emailHandlerManager.ResendEmailNotifications(It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns(Task.FromResult(responses));

            var result = emailController.ResendEmailNotifications(this.applicationName, this.notificationIds);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.emailHandlerManager.Verify(mgr => mgr.ResendEmailNotifications(It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
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
    }
}
