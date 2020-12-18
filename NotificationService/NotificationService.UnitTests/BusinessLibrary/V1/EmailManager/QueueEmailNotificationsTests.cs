// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinesLibrary.V1.EmailManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Microsoft.Azure.Storage.Queue;
    using Moq;
    using NotificationService.Contracts;
    using NotificationService.Data;
    using NotificationService.UnitTests.BusinessLibrary.V1.EmailManager;
    using NUnit.Framework;

    /// <summary>
    /// Tests for QueueEmailNotifications method of Email Manager.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class QueueEmailNotificationsTests : EmailManagerTestsBase
    {
        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup() => this.SetupTestBase();

        /// <summary>
        /// Tests for QueueEmailNotifications method for valid inputs.
        /// </summary>
        [Test]
        public void QueueEmailNotificationsTestValidInput()
        {
            Task<IList<NotificationResponse>> result = this.EmailHandlerManager.QueueEmailNotifications(this.ApplicationName, this.EmailNotificationItems);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).CreateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>(), It.IsAny<string>()), Times.Once);
            this.CloudStorageClient.Verify(csa => csa.QueueCloudMessages(It.IsAny<CloudQueue>(), It.IsAny<IEnumerable<string>>(), null), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for QueueEmailNotifications method for invalid inputs.
        /// </summary>
        [Test]
        public void QueueEmailNotificationsTestInvalidInput()
        {
            _ = Assert.ThrowsAsync<ArgumentException>(async () => await this.EmailHandlerManager.QueueEmailNotifications(null, this.EmailNotificationItems));
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailHandlerManager.QueueEmailNotifications(this.ApplicationName, null));
        }
    }
}
