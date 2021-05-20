// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Controllers.V1.MeetingInviteController
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NotificationHandler.Controllers;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models.Request;
    using NUnit.Framework;

    /// <summary>
    /// Resend Meeting invites by notificationIds class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ResendMeetingInvitesByNotificationIds
    {
        /// <summary>
        /// ApplicationName test constant.
        /// </summary>
        private readonly string applicationName = "TestApp";

        /// <summary>
        /// notificationIds test constant.
        /// </summary>
        private readonly string[] notificationIds = new string[] { "id-01", "id-02" };

        /// <summary>
        /// Mocked instace variable for EmailHandlerManager.
        /// </summary>
        private Mock<IEmailHandlerManager> emailHandlerManager;

        /// <summary>
        /// logger instance variable.
        /// </summary>
        private Mock<ILogger> logger;

        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.emailHandlerManager = new Mock<IEmailHandlerManager>();
            this.logger = new Mock<ILogger>();
            this.logger.Setup(c => c.TraceInformation(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).Verifiable();
        }

        /// <summary>
        /// Test Resend Meeting invites null/empty application and notificationIds.
        /// </summary>
        [Test]
        public void ResendMeetingInvites_Failed_InputValidation()
        {
            MeetingInviteController classUnderTest = new MeetingInviteController(this.emailHandlerManager.Object, this.logger.Object);
            Task<IActionResult> res;
            try
            {
                res = classUnderTest.ResendMeetingInvites(null, null);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentNullException);
            }

            try
            {
                res = classUnderTest.ResendMeetingInvites(this.applicationName, null);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentNullException);
            }

            try
            {
                res = classUnderTest.ResendMeetingInvites(this.applicationName, new string[] { });
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentNullException);
            }
        }

        /// <summary>
        /// Resend Meeting Invites successfully.
        /// </summary>
        [Test]
        public void ResendMeetingInvites_Success()
        {
            _ = this.emailHandlerManager.Setup(c => c.ResendNotifications(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                .ReturnsAsync(this.GetResendMeetingInvitesResponse());

            MeetingInviteController classUnderTest = new MeetingInviteController(this.emailHandlerManager.Object, this.logger.Object);
            var res = classUnderTest.ResendMeetingInvites(this.applicationName, this.notificationIds);
            Assert.AreEqual(res.Status.ToString(), "RanToCompletion");
            this.emailHandlerManager.Verify(c => c.ResendNotifications(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<NotificationType>(), It.IsAny<bool>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Method to create the MeetingInvites resend response object.
        /// </summary>
        /// <returns><see cref="IList{NotificationResponse}"/>.</returns>
        private IList<NotificationResponse> GetResendMeetingInvitesResponse()
        {
            return this.notificationIds.Select(c => new NotificationResponse() { NotificationId = c, Status = NotificationItemStatus.Queued }).ToList();
        }

        /// <summary>
        /// Tests for ResendMeetingNotifications By Date Range.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ResendEmailNotificationsByDateRangeTest_ValidInput()
        {
            MeetingInviteController classUnderTest = new MeetingInviteController(this.emailHandlerManager.Object, this.logger.Object);
            IList<NotificationResponse> responses = new List<NotificationResponse>() { new NotificationResponse() { NotificationId = Guid.NewGuid().ToString(), Status = NotificationItemStatus.Queued, } };
            var dateRange = new DateTimeRange()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(4),
            };

            _ = this.emailHandlerManager
                .Setup(emailHandlerManager => emailHandlerManager.ResendMeetingNotificationsByDateRange(It.IsAny<string>(), It.IsAny<DateTimeRange>()))
                .Returns(Task.FromResult(responses));

            var result = await classUnderTest.ResendMeetingNotificationsByDateRange(this.applicationName, dateRange);
            var res = (AcceptedResult)result;
            Assert.IsNotNull(res.Value);
            Assert.IsTrue(((List<NotificationResponse>)res.Value).Count == 1);
        }

        /// <summary>
        /// Tests for ResendMeetingNotifications By Date Range.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ResendEmailNotificationsByDateRangeTest_ValidInput_NoRecordToProcess()
        {
            MeetingInviteController classUnderTest = new MeetingInviteController(this.emailHandlerManager.Object, this.logger.Object);
            IList<NotificationResponse> responses = new List<NotificationResponse>();
            var dateRange = new DateTimeRange()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(4),
            };

            _ = this.emailHandlerManager
                .Setup(emailHandlerManager => emailHandlerManager.ResendMeetingNotificationsByDateRange(It.IsAny<string>(), It.IsAny<DateTimeRange>()))
                .Returns(Task.FromResult(responses));

            var result = await classUnderTest.ResendMeetingNotificationsByDateRange(this.applicationName, dateRange);
            var res = (AcceptedResult)result;
            Assert.IsNotNull(res.Value);
            responses = null;
            _ = this.emailHandlerManager
               .Setup(emailHandlerManager => emailHandlerManager.ResendMeetingNotificationsByDateRange(It.IsAny<string>(), It.IsAny<DateTimeRange>()))
               .Returns(Task.FromResult(responses));
            result = await classUnderTest.ResendMeetingNotificationsByDateRange(this.applicationName, dateRange);
            res = (AcceptedResult)result;
            Assert.IsNull(res.Value);
        }

        /// <summary>
        /// Tests for ResendMeetingNotifications By Date Range with invalid input.
        /// </summary>
        [Test]
        public void ResendMeetingNotificationsByDateRangeTest_InvalidInput()
        {
            MeetingInviteController classUnderTest = new MeetingInviteController(this.emailHandlerManager.Object, this.logger.Object);
            _ = Assert.ThrowsAsync<ArgumentException>(async () => await classUnderTest.ResendMeetingNotificationsByDateRange(null, null));
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await classUnderTest.ResendMeetingNotificationsByDateRange(this.applicationName, null));
        }
    }
}
