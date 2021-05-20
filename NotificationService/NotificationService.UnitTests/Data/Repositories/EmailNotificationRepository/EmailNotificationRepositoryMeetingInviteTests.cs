// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Moq;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models.Request;
    using NUnit.Framework;

    /// <summary>
    /// Tests for MeetingInviteTests method of Email Notification Repository.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EmailNotificationRepositoryMeetingInviteTests : EmailNotificationRepositoryTestsBase
    {
        /// <summary>
        /// DateRange object.
        /// </summary>
        private readonly DateTimeRange dateRange = new DateTimeRange
        {
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddHours(2),
        };
        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup() => this.SetupTestBase();

        /// <summary>
        /// Tests for CreateMeetingNotificationItemEntities method for invalid inputs.
        /// </summary>
        [Test]
        public void CreateMeetingNotificationItemEntitiesTestInvalidInput()
        {
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailNotificationRepository.CreateMeetingNotificationItemEntities(null));
        }

        /// <summary>
        /// Tests for CreateMeetingNotificationItemEntities method for valid inputs.
        /// </summary>
        [Test]
        public void CreateMeetingNotificationItemEntitiesTestValidInput()
        {
            var result = this.EmailNotificationRepository.CreateMeetingNotificationItemEntities(this.meetingNotificationEntities);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.CosmosDBQueryClient.Verify(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), this.MeetingHistoryContainerName), Times.Once);
            this.MeetingHistoryContainer.Verify(container => container.CreateItemAsync(It.IsAny<MeetingNotificationItemCosmosDbEntity>(), null, null, It.IsAny<CancellationToken>()), Times.Exactly(2));
            Assert.Pass();
        }

        /// <summary>
        /// Tests for UpdateMeetingNotificationItemEntities method for invalid inputs.
        /// </summary>
        [Test]
        public void UpdateMeetingNotificationItemEntitiesTestInvalidInput()
        {
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailNotificationRepository.UpdateMeetingNotificationItemEntities(null));
        }

        /// <summary>
        /// Tests for UpdateEmailNotificationItemEntities method for valid inputs.
        /// </summary>
        [Test]
        public void UpdateMeetingNotificationItemEntitiesTestValidInput()
        {
            var result = this.EmailNotificationRepository.UpdateMeetingNotificationItemEntities(this.meetingNotificationEntities);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.CosmosDBQueryClient.Verify(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), this.MeetingHistoryContainerName), Times.Once);
            this.MeetingHistoryContainer.Verify(container => container.UpsertItemAsync(It.IsAny<MeetingNotificationItemCosmosDbEntity>(), null, null, It.IsAny<CancellationToken>()), Times.Exactly(2));
            Assert.Pass();
        }

        /// <summary>
        /// Tests for GetMeetingNotificationItemEntities method for invalid inputs.
        /// </summary>
        [Test]
        public void GetMeetingNotificationItemEntitiesTestInvalidInput()
        {
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailNotificationRepository.GetMeetingNotificationItemEntities(null));
        }

        /// <summary>
        /// Tests for GetMeetingNotificationItemEntities method for valid inputs.
        /// </summary>
        [Test]
        public void GetMeetingNotificationItemEntitiesTestValidInput()
        {
            var result = this.EmailNotificationRepository.GetMeetingNotificationItemEntities(new List<string>() { Guid.NewGuid().ToString() });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.CosmosDBQueryClient.Verify(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), this.MeetingHistoryContainerName), Times.Once);
            this.MeetingHistoryContainer.Verify(container => container.GetItemLinqQueryable<MeetingNotificationItemCosmosDbEntity>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for GetMeetingNotificationItemEntity method for invalid inputs.
        /// </summary>
        [Test]
        public void GetMeetingNotificationItemEntityTestInvalidInput()
        {
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailNotificationRepository.GetMeetingNotificationItemEntity(null));
        }

        /// <summary>
        /// Tests for GetMeetingNotificationItemEntity method for valid inputs.
        /// </summary>
        [Test]
        public void GetMeetingNotificationItemEntityTestValidInput()
        {
            var result = this.EmailNotificationRepository.GetMeetingNotificationItemEntity(Guid.NewGuid().ToString());
            this.CosmosDBQueryClient.Verify(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), this.MeetingHistoryContainerName), Times.Once);
            this.MeetingHistoryContainer.Verify(container => container.GetItemLinqQueryable<MeetingNotificationItemCosmosDbEntity>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for Get Meeting Notifications for Reporting.
        /// </summary>
        [Test]
        public void GetFilteredMeetingNotificationTests()
        {
            var request = new NotificationReportRequest()
            {
                NotificationStatusFilter = new List<NotificationItemStatus> { NotificationItemStatus.Sent, NotificationItemStatus.Processing },
                NotificationPriorityFilter = new List<NotificationPriority> { NotificationPriority.High },
                NotificationIdsFilter = new List<string> { "1" },
                TrackingIdsFilter = new List<string> { "trackingid" },
                AccountsUsedFilter = new List<string> { "gtauser" },
                ApplicationFilter = new List<string>() { "test", "SelectedApp", },
                CreatedDateTimeStart = "2020-07-21",
            };
            var faultedResult = this.EmailNotificationRepository.GetMeetingInviteNotifications(null);
            Assert.AreEqual(faultedResult.Status.ToString(), "Faulted");

            Expression<Func<MeetingNotificationItemCosmosDbEntity, bool>> filterExpression = n => true;
            Expression<Func<MeetingNotificationItemCosmosDbEntity, DateTime>> sortExpression = n => n.CreatedDateTime;

            Expression<Func<MeetingNotificationItemCosmosDbEntity, MeetingNotificationItemCosmosDbEntity>> selectExpression = n => new MeetingNotificationItemCosmosDbEntity() { NotificationId = n.NotificationId };
            var result = this.EmailNotificationRepository.GetMeetingInviteNotifications(request);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");

            this.CosmosDBQueryClient.Verify(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), this.MeetingHistoryContainerName), Times.Once);
            this.MeetingHistoryContainer.Verify(container => container.GetItemLinqQueryable<MeetingNotificationItemCosmosDbEntity>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()), Times.Once);
            Assert.Pass();
        }


        /// <summary>
        /// Tests for Get Meeting Notifications for Reporting.
        /// </summary>
        [Test]
        public async Task GetMeetingNotificationItemEntitiesBetweenDatesTestsCosmosDb()
        {
            var statusList = new List<NotificationItemStatus>() { NotificationItemStatus.Failed };
            var application = "TestApp";

            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailNotificationRepository.GetPendingOrFailedMeetingNotificationsByDateRange(null, application, statusList));

            Expression<Func<MeetingNotificationItemCosmosDbEntity, bool>> filterExpression = n => true;
            Expression<Func<MeetingNotificationItemCosmosDbEntity, DateTime>> sortExpression = n => n.CreatedDateTime;
            Expression<Func<MeetingNotificationItemCosmosDbEntity, MeetingNotificationItemCosmosDbEntity>> selectExpression = n => new MeetingNotificationItemCosmosDbEntity() { NotificationId = n.NotificationId };

            var result = await this.EmailNotificationRepository.GetPendingOrFailedMeetingNotificationsByDateRange(this.dateRange, null, statusList);
            Assert.IsNotNull(result);

            var faultedResult = await this.EmailNotificationRepository.GetPendingOrFailedMeetingNotificationsByDateRange(this.dateRange, application, null);
            Assert.IsNotNull(result);

            result = await this.EmailNotificationRepository.GetPendingOrFailedMeetingNotificationsByDateRange(this.dateRange, application, statusList);
            Assert.IsNotNull(result);

            this.CosmosDBQueryClient.Verify(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), this.MeetingHistoryContainerName), Times.Once);
            this.MeetingHistoryContainer.Verify(container => container.GetItemLinqQueryable<MeetingNotificationItemCosmosDbEntity>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()), Times.Exactly(3));
            Assert.Pass();

        }
    }
}