// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Microsoft.Azure.Cosmos;
    using Moq;
    using NotificationService.Contracts;
    using NUnit.Framework;

    /// <summary>
    /// Tests for GetEmailNotificationItemEntities method of Email Notification Repository.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetEmailNotificationItemEntitiesTests : EmailNotificationRepositoryTestsBase
    {
        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup() => this.SetupTestBase();

        /// <summary>
        /// Tests for GetEmailNotificationItemEntities method for invalid inputs.
        /// </summary>
        [Test]
        public void GetEmailNotificationItemEntitiesTestInvalidInput()
        {
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailNotificationRepository.GetEmailNotificationItemEntities(null));
        }

        /// <summary>
        /// Tests for UpdateEmailNotificationItemEntities method for valid inputs.
        /// </summary>
        [Test]
        public void GetEmailNotificationItemEntitiesTestValidInput()
        {
            var result = this.EmailNotificationRepository.GetEmailNotificationItemEntities(new List<string>() { Guid.NewGuid().ToString() });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.CosmosDBQueryClient.Verify(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            this.CosmosContainer.Verify(container => container.GetItemLinqQueryable<EmailNotificationItemEntity>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for Get Email Notifications for Reporting.
        /// </summary>
        [Test]
        public void GetFilteredEmailNotificationTests()
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
            var faultedResult = this.EmailNotificationRepository.GetEmailNotifications(null);
            Assert.AreEqual(faultedResult.Status.ToString(), "Faulted");

            Expression<Func<EmailNotificationItemEntity, bool>> filterExpression = n => true;
            Expression<Func<EmailNotificationItemEntity, DateTime>> sortExpression = n => n.CreatedDateTime;

            Expression<Func<EmailNotificationItemEntity, EmailNotificationItemEntity>> selectExpression = n => new EmailNotificationItemEntity() { NotificationId = n.NotificationId };
            var result = this.EmailNotificationRepository.GetEmailNotifications(request);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");

            this.CosmosDBQueryClient.Verify(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            this.CosmosContainer.Verify(container => container.GetItemLinqQueryable<EmailNotificationItemEntity>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()), Times.Once);
            Assert.Pass();
        }
    }
}
