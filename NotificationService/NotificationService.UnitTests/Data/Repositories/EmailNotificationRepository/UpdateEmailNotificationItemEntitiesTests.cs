// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Data.Repositories
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Moq;
    using NotificationService.Contracts;
    using NUnit.Framework;

    /// <summary>
    /// Tests for UpdateEmailNotificationItemEntities method of Email Notification Repository.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UpdateEmailNotificationItemEntitiesTests : EmailNotificationRepositoryTestsBase
    {
        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup() => this.SetupTestBase();

        /// <summary>
        /// Tests for UpdateEmailNotificationItemEntities method for invalid inputs.
        /// </summary>
        [Test]
        public void UpdateEmailNotificationItemEntitiesTestInvalidInput()
        {
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailNotificationRepository.UpdateEmailNotificationItemEntities(null));
        }

        /// <summary>
        /// Tests for UpdateEmailNotificationItemEntities method for valid inputs.
        /// </summary>
        [Test]
        public void UpdateEmailNotificationItemEntitiesTestValidInput()
        {
            var result = this.EmailNotificationRepository.UpdateEmailNotificationItemEntities(this.NotificationEntities);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.CosmosDBQueryClient.Verify(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), this.MailHistoryContainerName), Times.Once);
            this.EmailHistoryContainer.Verify(container => container.UpsertItemAsync(It.IsAny<EmailNotificationItemCosmosDbEntity>(), null, null, It.IsAny<CancellationToken>()), Times.Exactly(2));
            Assert.Pass();
        }
    }
}
