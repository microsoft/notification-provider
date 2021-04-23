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
    /// Tests for CreateEmailNotificationItemEntities method of Email Notification Repository.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateEmailNotificationItemEntitiesTests : EmailNotificationRepositoryTestsBase
    {
        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup() => this.SetupTestBase();

        /// <summary>
        /// Tests for CreateEmailNotificationItemEntities method for invalid inputs.
        /// </summary>
        [Test]
        public void CreateEmailNotificationItemEntitiesTestInvalidInput()
        {
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailNotificationRepository.CreateEmailNotificationItemEntities(null));
        }

        /// <summary>
        /// Tests for CreateEmailNotificationItemEntities method for valid inputs.
        /// </summary>
        [Test]
        public void CreateEmailNotificationItemEntitiesTestValidInput()
        {
            var result = this.EmailNotificationRepository.CreateEmailNotificationItemEntities(this.NotificationEntities);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.CosmosDBQueryClient.Verify(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), this.MailHistoryContainerName), Times.Once);
            this.EmailHistoryContainer.Verify(container => container.CreateItemAsync(It.IsAny<EmailNotificationItemCosmosDbEntity>(), null, null, It.IsAny<CancellationToken>()), Times.Exactly(2));
            Assert.Pass();
        }
    }
}
