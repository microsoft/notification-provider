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
    using NotificationService.Contracts.Models;
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

        private readonly MeetingNotificationItem[] meetingNotificationItems = new MeetingNotificationItem[]
            {
                new MeetingNotificationItem() { RequiredAttendees = "user@contoso.com" },
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

        /// <summary>
        /// Tests for QueueEmailNotifications method for valid inputs.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task QueueMeetingNotificationsTestValidInput()
        {
            _ = this.emailHandlerManager.Setup(x => x.QueueMeetingNotifications(It.IsAny<string>(), It.IsAny<MeetingNotificationItem[]>())).ReturnsAsync(new List<NotificationResponse> { new NotificationResponse { NotificationId = "NotificationId" } });
            MeetingInviteController meetinginviteController = new MeetingInviteController(this.emailHandlerManager.Object, this.logger);
            IList<NotificationResponse> responses = new List<NotificationResponse>();
            var result = await meetinginviteController.QueueMeetingNotifications(this.applicationName, this.meetingNotificationItems);
            Assert.NotNull(result);
            var res = result as AcceptedResult;
            var items = res.Value as List<NotificationResponse>;
            Assert.IsTrue(items.Count == 1 && items[0].NotificationId == "NotificationId");
            this.emailHandlerManager.Verify(mgr => mgr.QueueMeetingNotifications(It.IsAny<string>(), It.IsAny<MeetingNotificationItem[]>()), Times.Once);
            Assert.Pass();
        }
    }
}
