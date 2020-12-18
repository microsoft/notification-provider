// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Controllers.V1.MeetingInviteController
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
    /// Tests for ProcessQueuedMeetingNotifications method of Email Controller.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProcessQueueMeetingNotificationsTests
    {
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
        /// Tests for ProcessQueuedEmailNotifications method for valid inputs.
        /// </summary>
        [Test]
        public void ProcessQueuedEmailNotificationsTestValidInput()
        {
            MeetingInviteController meetingInviteController = new MeetingInviteController(this.emailServiceManager.Object, this.logger);
            IList<NotificationResponse> responses = new List<NotificationResponse>();
            string applicationName = "TestApp";
            QueueNotificationItem queueNotificationItem = new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() } };

            _ = this.emailServiceManager
                .Setup(emailServiceManager => emailServiceManager.ProcessMeetingNotifications(It.IsAny<string>(), It.IsAny<QueueNotificationItem>()))
                .Returns(Task.FromResult(responses));

            Task<IList<NotificationResponse>> result = meetingInviteController.ProcessQueuedMeetingNotifications(applicationName, queueNotificationItem);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.emailServiceManager.Verify(mgr => mgr.ProcessMeetingNotifications(It.IsAny<string>(), It.IsAny<QueueNotificationItem>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for ResendEmailNotifications method for invalid inputs.
        /// </summary>
        [Test]
        public void ProcessQueuedEmailNotificationsTestInvalidInput()
        {
            MeetingInviteController meetingInviteController = new MeetingInviteController(this.emailServiceManager.Object, this.logger);
            QueueNotificationItem queueNotificationItem = new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() } };

            _ = Assert.ThrowsAsync<ArgumentException>(async () => await meetingInviteController.ProcessQueuedMeetingNotifications(null, queueNotificationItem));
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await meetingInviteController.ProcessQueuedMeetingNotifications("TestApp", null));
        }
    }
}
