// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Contracts.Entities.Web.WebNotificationItemEntityTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities.Web;
    using NotificationService.Contracts.Models.Graph;
    using NotificationService.Contracts.Models.Web.Response;
    using NUnit.Framework;

    /// <summary>
    /// Nothing
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class WebNotificationItemEntity_ToWebNotificationsTests
    {
        private WebNotificationItemEntity webNotificationItemEntity;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            this.webNotificationItemEntity = new WebNotificationItemEntity
            {
                NotificationId = Guid.NewGuid().ToString(),
                Title = "Notice Title",
                Body = "This is a notice body.",
                Properties = new Dictionary<string, string> { { "DeepLink", "https://www.microsoft.com" }, },
                Priority = NotificationPriority.Normal,
                ReadStatus = NotificationReadStatus.New,
                TrackingId = Guid.NewGuid().ToString(),
                SendOnUtcDate = DateTime.UtcNow,
                PublishOnUTCDate = DateTime.UtcNow,
                ExpiresOnUTCDate = DateTime.UtcNow.AddDays(1),
                AppNotificationType = "Test Type",
                Sender = new Person
                {
                    Name = "Software Engineer",
                    Email = "abc@xyz.com",
                    ObjectIdentifier = Guid.NewGuid().ToString(),
                },
            };
        }

        /// <summary>
        /// Tests ToWebNotification method.
        /// </summary>
        [Test]
        public void ToWebNoticationValidEntity()
        {
            WebNotification webNotification = this.webNotificationItemEntity.ToWebNotification();
            Assert.IsTrue(webNotification.NotificationId.Equals(this.webNotificationItemEntity.NotificationId, StringComparison.Ordinal));
            Assert.IsTrue(webNotification.Title.Equals(this.webNotificationItemEntity.Title, StringComparison.Ordinal));
            Assert.IsTrue(webNotification.Body.Equals(this.webNotificationItemEntity.Body, StringComparison.Ordinal));
            Assert.AreEqual(webNotification.Properties.Count, this.webNotificationItemEntity.Properties.Count);
            Assert.IsTrue(webNotification.Sender.Name.Equals(this.webNotificationItemEntity.Sender.Name, StringComparison.Ordinal));
            Assert.IsTrue(webNotification.Sender.Email.Equals(this.webNotificationItemEntity.Sender.Email, StringComparison.Ordinal));
            Assert.IsTrue(webNotification.Sender.ObjectIdentifier.Equals(this.webNotificationItemEntity.Sender.ObjectIdentifier, StringComparison.Ordinal));
            Assert.IsTrue(webNotification.ExpiresOnUTCDate == this.webNotificationItemEntity.ExpiresOnUTCDate);
            Assert.IsTrue(webNotification.PublishOnUTCDate == this.webNotificationItemEntity.PublishOnUTCDate);
            Assert.IsTrue(webNotification.Priority == this.webNotificationItemEntity.Priority);
            Assert.IsTrue(webNotification.ReadStatus == this.webNotificationItemEntity.ReadStatus);
            Assert.IsTrue(webNotification.AppNotificationType.Equals(this.webNotificationItemEntity.AppNotificationType, StringComparison.OrdinalIgnoreCase));
        }
    }
}
