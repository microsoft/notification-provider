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
    using NotificationService.Contracts.Models;
    using NotificationService.Controllers;
    using NUnit.Framework;

    /// <summary>
    /// Tests for SendEmailNotifications method of Email Controller.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SendMeetingNotificationsTest
    {
        private readonly string applicationName = "TestApp";
        private readonly MeetingNotificationItem[] meetingInvitesItem = new MeetingNotificationItem[]
        {
            new MeetingNotificationItem() { From = "user@contoso.com", RequiredAttendees = "test@contoso.com" },
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
            MeetingInviteController meetingInviteController = new MeetingInviteController(this.emailServiceManager.Object, this.logger);
            IList<NotificationResponse> responses = new List<NotificationResponse>();

            _ = this.emailServiceManager
                .Setup(emailServiceManager => emailServiceManager.SendMeetingInvites(It.IsAny<string>(), It.IsAny<MeetingNotificationItem[]>()))
                .Returns(Task.FromResult(responses));

            var result = meetingInviteController.SendMeetingInvites(this.applicationName, this.meetingInvitesItem);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.emailServiceManager.Verify(mgr => mgr.SendMeetingInvites(It.IsAny<string>(), It.IsAny<MeetingNotificationItem[]>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for SendEmailNotifications method for invalid inputs.
        /// </summary>
        [Test]
        public void SendEmailNotificationsTestInvalidInput()
        {
            MeetingInviteController meetingInviteController = new MeetingInviteController(this.emailServiceManager.Object, this.logger);

            _ = Assert.ThrowsAsync<ArgumentException>(async () => await meetingInviteController.SendMeetingInvites(null, this.meetingInvitesItem));
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await meetingInviteController.SendMeetingInvites(this.applicationName, null));
        }
    }
}
