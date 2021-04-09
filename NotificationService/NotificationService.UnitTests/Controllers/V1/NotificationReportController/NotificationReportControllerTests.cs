// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Controllers.V1.NotificationReportController
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Moq;
    using NotificationHandler.Controllers;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models.Reports;
    using NUnit.Framework;

    /// <summary>
    /// Tests for Notification Report Controller.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NotificationReportControllerTests
    {
        private readonly NotificationReportRequest request = new NotificationReportRequest()
        {
            NotificationStatusFilter = new List<NotificationItemStatus> { NotificationItemStatus.Sent, NotificationItemStatus.Processing },
            NotificationPriorityFilter = new List<NotificationPriority> { NotificationPriority.High },
            NotificationIdsFilter = new List<string> { "1" },
            TrackingIdsFilter = new List<string> { "trackingId" },
            AccountsUsedFilter = new List<string> { "gtauser" },
            ApplicationFilter = new List<string>() { "test", "app1", },
            CreatedDateTimeStart = "2020-07-21",
            Token = null,
        };

        private readonly string applicationName = "TestApp";
        private readonly string notificationId = Guid.NewGuid().ToString();
        private Mock<INotificationReportManager> notificationReportManager;
        private ILogger logger;

        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.notificationReportManager = new Mock<INotificationReportManager>();
            this.logger = new Mock<ILogger>().Object;
        }

        /// <summary>
        /// test case for get report notification.
        /// </summary>
        [Test]
        public void GetReportNotificationsTest()
        {
            NotificationReportController notificationReportController = new NotificationReportController(this.notificationReportManager.Object, this.logger);
            IList<NotificationReportResponse> response = new List<NotificationReportResponse>();
            Tuple<IList<NotificationReportResponse>, TableContinuationToken> notificationResponses = new Tuple<IList<NotificationReportResponse>, TableContinuationToken>(response, null);
            _ = this.notificationReportManager
                .Setup(notificationReportManager => notificationReportManager.GetReportNotifications(It.IsAny<NotificationReportRequest>()))
                .Returns(Task.FromResult(notificationResponses));
            var result = notificationReportController.GetReportNotifications(this.request);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.notificationReportManager.Verify(mgr => mgr.GetReportNotifications(this.request), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// test case for get all templates.
        /// </summary>
        [Test]
        public void GetAllTemplateEntitiesTest()
        {
            NotificationReportController notificationReportController = new NotificationReportController(this.notificationReportManager.Object, this.logger);
            IList<MailTemplateInfo> mailTemplatesInfo = new List<MailTemplateInfo>();
            _ = this.notificationReportManager
                .Setup(notificationReportManager => notificationReportManager.GetAllTemplateEntities(It.IsAny<string>()))
                .Returns(Task.FromResult(mailTemplatesInfo));
            var result = notificationReportController.GetAllTemplateEntities(this.applicationName);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.notificationReportManager.Verify(mgr => mgr.GetAllTemplateEntities(this.applicationName), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// test case for get NotificationMessage.
        /// </summary>
        [Test]
        public void GetNotificationMessageTest()
        {
            NotificationReportController notificationReportController = new NotificationReportController(this.notificationReportManager.Object, this.logger);
            EmailMessage notificationMessage = new EmailMessage();
            _ = this.notificationReportManager
                .Setup(notificationReportManager => notificationReportManager.GetNotificationMessage(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(notificationMessage));
            var result = notificationReportController.GetNotificationMessage(this.applicationName, this.notificationId);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.notificationReportManager.Verify(mgr => mgr.GetNotificationMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// test case for get NotificationMessage.
        /// </summary>
        [Test]
        public void GetMeetingNotificationMessageTest()
        {
            NotificationReportController notificationReportController = new NotificationReportController(this.notificationReportManager.Object, this.logger);
            MeetingInviteMessage notificationMessage = new MeetingInviteMessage();
            _ = this.notificationReportManager
                .Setup(notificationReportManager => notificationReportManager.GetMeetingNotificationMessage(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(notificationMessage));
            var result = notificationReportController.GetMeetingNotificationMessage(this.applicationName, this.notificationId);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.notificationReportManager.Verify(mgr => mgr.GetMeetingNotificationMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// test case for get meeting invite report notification.
        /// </summary>
        [Test]
        public void GetMeetingInviteReportNotificationsTest()
        {
            NotificationReportController notificationReportController = new NotificationReportController(this.notificationReportManager.Object, this.logger);
            IList<MeetingInviteReportResponse> response = new List<MeetingInviteReportResponse>();
            Tuple<IList<MeetingInviteReportResponse>, TableContinuationToken> notificationResponses = new Tuple<IList<MeetingInviteReportResponse>, TableContinuationToken>(response, null);
            _ = this.notificationReportManager
                .Setup(notificationReportManager => notificationReportManager.GetMeetingInviteReportNotifications(It.IsAny<NotificationReportRequest>()))
                .Returns(Task.FromResult(notificationResponses));
            var result = notificationReportController.GetMeetingInviteReportNotifications(this.request);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.notificationReportManager.Verify(mgr => mgr.GetMeetingInviteReportNotifications(this.request), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for GetReportNotifications method.
        /// </summary>

        // [Test]
        // public void GetReportNotificationsTest()
        // {
        //    NotificationReportController notificationReportController = new NotificationReportController(this.notificationReportManager.Object, this.logger);
        //    IList<NotificationReportResponse> responses = new List<NotificationReportResponse>();
        //    var request = new NotificationReportRequest()
        //    {
        //        NotificationStatusFilter = new List<NotificationItemStatus> { NotificationItemStatus.Sent, NotificationItemStatus.Processing },
        //        MailSensitivityFilter = new List<MailSensitivity> { MailSensitivity.Normal },
        //        NotificationPriorityFilter = new List<NotificationPriority> { NotificationPriority.High },
        //        NotificationTypeFilter = new List<NotificationType> { NotificationType.Mail },
        //        NotificationIdsFilter = new List<string> { "1" },
        //        AccountsUsedFilter = new List<string> { "gtauser" },
        //        ApplicationFilter = new List<string>() { "test", "app1", },
        //        CreatedDateTimeStart = "2020-07-21",
        //    };
        //    List<NotificationReportResponse> dbEntities = new List<NotificationReportResponse>();
        //    dbEntities.Add(new NotificationReportResponse() { NotificationId = "1", Application = "SelectedApp" });
        //    dbEntities.Add(new NotificationReportResponse() { NotificationId = "2", Application = "SelectedApp" });
        //    var tuple = Tuple.Create<IList<NotificationReportResponse>, TableContinuationToken>(dbEntities, new TableContinuationToken { NextPartitionKey = "a", NextRowKey = "b" });
        //    _ = this.notificationReportManager
        //        .Setup(reportManager => reportManager.GetReportNotifications(request)).Returns(Task.FromResult(tuple));
        //    Task<IActionResult> result = notificationReportController.GetReportNotifications(request);
        //    Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
        //    this.notificationReportManager.Verify(mgr => mgr.GetReportNotifications(request), Times.Once);
        //    Assert.Pass();
        // }
    }
}
