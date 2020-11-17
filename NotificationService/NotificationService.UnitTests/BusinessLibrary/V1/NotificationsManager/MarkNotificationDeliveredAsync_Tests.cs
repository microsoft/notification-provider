// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.V1.NotificationsManager_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Moq;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Entities.Web;
    using NUnit.Framework;

    /// <summary>
    /// Test class.
    /// </summary>
    /// <seealso cref="NotificationsTestsBase" />
    [ExcludeFromCodeCoverage]
    public class MarkNotificationDeliveredAsync_Tests : NotificationsTestsBase
    {
        /// <summary>
        /// Setups the base.
        /// </summary>
        [SetUp]
        public override void SetupBase()
        {
            base.SetupBase();
        }

        /// <summary>
        /// Marks the notification delivered asynchronous null notification ids.
        /// </summary>
        [Test]
        public void MarkNotificationDeliveredAsync_NullNotificationIdsNoExceptions()
        {
            var task = this.NotificationManager.MarkNotificationDeliveredAsync(null, NotificationDeliveryChannel.Web, throwExceptions: false);
            Assert.IsTrue(task.Status == TaskStatus.RanToCompletion);
        }

        /// <summary>
        /// Marks the notification delivered asynchronous null notification ids with exceptions.
        /// </summary>
        [Test]
        public void MarkNotificationDeliveredAsync_NullNotificationIdsWithExceptions()
        {
            var ex = Assert.ThrowsAsync<ArgumentNullException>(() => this.NotificationManager.MarkNotificationDeliveredAsync(null, NotificationDeliveryChannel.Web));
            Assert.IsTrue(ex.ParamName.Equals("notificationIds", StringComparison.Ordinal));
        }

        /// <summary>
        /// Marks the notification delivered asynchronous valid notification ids.
        /// </summary>
        [Test]
        public async Task MarkNotificationDeliveredAsync_ValidNotificationIds()
        {
            this.NotificationId = "Notification Id #2";
            EntityCollection<WebNotificationItemEntity> collection = new EntityCollection<WebNotificationItemEntity>
            {
                Items = this.NotificationEntities.Where(note => note.NotificationId == this.NotificationId && note.ExpiresOnUTCDate > DateTime.UtcNow && note.PublishOnUTCDate < DateTime.UtcNow),
            };
            _ = this.notificationsRepositoryMock.Setup<Task<EntityCollection<WebNotificationItemEntity>>>(rp => rp.ReadAsync(It.IsAny<Expression<Func<WebNotificationItemEntity, bool>>>(), It.IsAny<Expression<Func<WebNotificationItemEntity, NotificationPriority>>>(), It.IsAny<string>(), It.IsAny<int>())).
                Returns(Task.FromResult(collection));
            _ = this.notificationsRepositoryMock.Setup(rp => rp.UpsertAsync(It.IsAny<IEnumerable<WebNotificationItemEntity>>())).ReturnsAsync(collection.Items.Select(it =>
            {
                it.DeliveredOnChannel = new Dictionary<NotificationDeliveryChannel, bool>
                    {
                        { NotificationDeliveryChannel.Web, true },
                    };
                return it;
            }));

            await this.NotificationManager.MarkNotificationDeliveredAsync(new List<string> { this.NotificationId }, NotificationDeliveryChannel.Web);
            this.notificationsRepositoryMock.Verify(rp => rp.ReadAsync(It.IsAny<Expression<Func<WebNotificationItemEntity, bool>>>(), It.IsAny<Expression<Func<WebNotificationItemEntity, NotificationPriority>>>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            this.notificationsRepositoryMock.Verify(rp => rp.UpsertAsync(It.IsAny<IEnumerable<WebNotificationItemEntity>>()), Times.Once);
        }
    }
}
