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
    /// Tests for QueueEmailNotifications method of Email Controller.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class QueueEmailNotificationsTests
    {
        private readonly EmailNotificationItem[] emailNotificationItems = new EmailNotificationItem[]
            {
                new EmailNotificationItem() { To = "user@contoso.com" },
            };

        private readonly string applicationName = "TestApp";
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
        /// Tests for QueueEmailNotifications method for valid inputs.
        /// </summary>
        [Test]
        public void QueueEmailNotificationsTestValidInput()
        {
            EmailController emailController = new EmailController(this.emailHandlerManager.Object, this.mailTemplateManager.Object, this.logger);
            IList<NotificationResponse> responses = new List<NotificationResponse>();

            _ = this.emailHandlerManager
                .Setup(emailHandlerManager => emailHandlerManager.QueueEmailNotifications(It.IsAny<string>(), It.IsAny<EmailNotificationItem[]>()))
                .Returns(Task.FromResult(responses));

            var result = emailController.QueueEmailNotifications(this.applicationName, this.emailNotificationItems);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.emailHandlerManager.Verify(mgr => mgr.QueueEmailNotifications(It.IsAny<string>(), It.IsAny<EmailNotificationItem[]>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for QueueEmailNotifications method for invalid inputs.
        /// </summary>
        [Test]
        public void QueueEmailNotificationsTestInvalidInput()
        {
            EmailController emailController = new EmailController(this.emailHandlerManager.Object, this.mailTemplateManager.Object, this.logger);

            _ = Assert.ThrowsAsync<ArgumentException>(async () => await emailController.QueueEmailNotifications(null, this.emailNotificationItems));
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await emailController.QueueEmailNotifications(this.applicationName, null));
        }
    }
}
