// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Controllers.V1.EmailController
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Moq;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Controllers;
    using NUnit.Framework;

    /// <summary>
    /// Tests for SendEmailNotifications method of Email Controller.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SendEmailNotificationsTests
    {
        private readonly string applicationName = "TestApp";
        private readonly EmailNotificationItem[] emailNotificationItems = new EmailNotificationItem[]
        {
                new EmailNotificationItem() { To = "user@contoso.com" },
        };

        private Mock<IEmailServiceManager> emailServiceManager;
        private ILogger logger;

        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.emailServiceManager = new Mock<IEmailServiceManager>();
            this.logger = new Mock<ILogger>().Object;
        }

        /// <summary>
        /// Tests for SendEmailNotifications method for valid inputs.
        /// </summary>
        [Test]
        public void SendEmailNotificationsTestValidInput()
        {
            EmailController emailController = new EmailController(this.emailServiceManager.Object, this.logger);
            IList<NotificationResponse> responses = new List<NotificationResponse>();

            _ = this.emailServiceManager
                .Setup(emailServiceManager => emailServiceManager.SendEmailNotifications(It.IsAny<string>(), It.IsAny<EmailNotificationItem[]>()))
                .Returns(Task.FromResult(responses));

            var result = emailController.SendEmailNotifications(this.applicationName, this.emailNotificationItems);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.emailServiceManager.Verify(mgr => mgr.SendEmailNotifications(It.IsAny<string>(), It.IsAny<EmailNotificationItem[]>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for SendEmailNotifications method for invalid inputs.
        /// </summary>
        [Test]
        public void SendEmailNotificationsTestInvalidInput()
        {
            EmailController emailController = new EmailController(this.emailServiceManager.Object, this.logger);

            _ = Assert.ThrowsAsync<ArgumentException>(async () => await emailController.SendEmailNotifications(null, this.emailNotificationItems));
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await emailController.SendEmailNotifications(this.applicationName, null));
        }
    }
}
