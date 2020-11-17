// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Contracts.Models.Web.Request.WebNotificationRequestItemTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities.Web;
    using NotificationService.Contracts.Models.Graph;
    using NotificationService.Contracts.Models.Web.Request;
    using NUnit.Framework;

    /// <summary>
    /// Test class
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class WebNotificationRequestItemTests_ToEntity
    {
        private WebNotificationRequestItem webNotificationRequestItem;

        /// <summary>
        /// Sets up test environment.
        /// </summary>
        [SetUp]
        public void TestSetup()
        {
            this.webNotificationRequestItem = new WebNotificationRequestItem
            {
                Title = "First Notification",
                Body = "This is a first notification",
                Properties = new Dictionary<string, string> { { "DeepLink", "https://www.microsoft.com" }, },
                TrackingId = Guid.NewGuid().ToString(),
                PublishOnUTCDate = DateTime.UtcNow,
                ExpiresOnUTCDate = DateTime.UtcNow.AddDays(1),
                AppNotificationType = "Test Type",
                Recipient = new Person
                {
                    Name = "Receiver Name",
                    Email = "abc@xyz.com",
                    ObjectIdentifier = Guid.NewGuid().ToString(),
                },
                Sender = new Person
                {
                    Name = "Sender Name",
                    Email = "xyz@abc.com",
                    ObjectIdentifier = Guid.NewGuid().ToString(),
                },
            };
        }

        /// <summary>
        /// Converts to entity_validwebnotificationrequestitemwithdefaultvalues.
        /// </summary>
        [Test]
        public void ToEntity_ValidWebNotificationRequestItemWithDefaultValues()
        {
            WebNotificationItemEntity webNotificationItemEntity = this.webNotificationRequestItem.ToEntity("Application 1");
            Assert.IsTrue(this.webNotificationRequestItem.Title.Equals(webNotificationItemEntity.Title, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Body.Equals(webNotificationItemEntity.Body, StringComparison.Ordinal));
            Assert.AreEqual(webNotificationItemEntity.Properties.Count, this.webNotificationRequestItem.Properties.Count);
            Assert.IsTrue(this.webNotificationRequestItem.TrackingId.Equals(webNotificationItemEntity.TrackingId, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Sender.Name.Equals(webNotificationItemEntity.Sender.Name, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Sender.Email.Equals(webNotificationItemEntity.Sender.Email, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Sender.ObjectIdentifier.Equals(webNotificationItemEntity.Sender.ObjectIdentifier, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Recipient.Name.Equals(webNotificationItemEntity.Recipient.Name, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Recipient.Email.Equals(webNotificationItemEntity.Recipient.Email, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Recipient.ObjectIdentifier.Equals(webNotificationItemEntity.Recipient.ObjectIdentifier, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.PublishOnUTCDate == webNotificationItemEntity.PublishOnUTCDate);
            Assert.IsTrue(this.webNotificationRequestItem.ExpiresOnUTCDate == webNotificationItemEntity.ExpiresOnUTCDate);
            Assert.IsTrue(webNotificationItemEntity.Priority == NotificationPriority.Normal);
            Assert.IsTrue(webNotificationItemEntity.NotifyType == NotificationType.Web);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(webNotificationItemEntity.Id));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(webNotificationItemEntity.NotificationId));
            Assert.IsTrue(webNotificationItemEntity.Application.Equals("Application 1", StringComparison.Ordinal));
            Assert.IsTrue(webNotificationItemEntity.AppNotificationType.Equals(webNotificationItemEntity.AppNotificationType, StringComparison.Ordinal));
        }

        /// <summary>
        /// Converts to entity_validwebnotificationrequestitemwithnondefaultvalues.
        /// </summary>
        [Test]
        public void ToEntity_ValidWebNotificationRequestItemWithNonDefaultValues()
        {
            this.webNotificationRequestItem.SendOnUtcDate = DateTime.UtcNow;
            this.webNotificationRequestItem.Priority = NotificationPriority.High;
            this.webNotificationRequestItem.TrackingId = null;
            this.webNotificationRequestItem.Properties = null;
            WebNotificationItemEntity webNotificationItemEntity = this.webNotificationRequestItem.ToEntity("Application 1");
            Assert.IsTrue(this.webNotificationRequestItem.Title.Equals(webNotificationItemEntity.Title, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Body.Equals(webNotificationItemEntity.Body, StringComparison.Ordinal));
            Assert.IsNull(webNotificationItemEntity.Properties);
            Assert.IsTrue(this.webNotificationRequestItem.Sender.Name.Equals(webNotificationItemEntity.Sender.Name, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Sender.Email.Equals(webNotificationItemEntity.Sender.Email, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Sender.ObjectIdentifier.Equals(webNotificationItemEntity.Sender.ObjectIdentifier, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Recipient.Name.Equals(webNotificationItemEntity.Recipient.Name, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Recipient.Email.Equals(webNotificationItemEntity.Recipient.Email, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.Recipient.ObjectIdentifier.Equals(webNotificationItemEntity.Recipient.ObjectIdentifier, StringComparison.Ordinal));
            Assert.IsTrue(this.webNotificationRequestItem.PublishOnUTCDate == webNotificationItemEntity.PublishOnUTCDate);
            Assert.IsTrue(webNotificationItemEntity.TrackingId == null);
            Assert.IsTrue(this.webNotificationRequestItem.ExpiresOnUTCDate == webNotificationItemEntity.ExpiresOnUTCDate);
            Assert.IsTrue(this.webNotificationRequestItem.SendOnUtcDate == webNotificationItemEntity.SendOnUtcDate);
            Assert.IsTrue(webNotificationItemEntity.Priority == NotificationPriority.High);
            Assert.IsTrue(webNotificationItemEntity.NotifyType == NotificationType.Web);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(webNotificationItemEntity.Id));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(webNotificationItemEntity.NotificationId));
            Assert.IsTrue(webNotificationItemEntity.Application.Equals("Application 1", StringComparison.Ordinal));
            Assert.IsTrue(webNotificationItemEntity.AppNotificationType.Equals(webNotificationItemEntity.AppNotificationType, StringComparison.Ordinal));
        }

        /// <summary>
        /// Converts to entity_validwebnotificationrequestitemwithinvalidapplicationname.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ToEntity_ValidWebNotificationRequestItemWithInvalidApplicationName(string applicationName)
        {
            var ex = Assert.Throws<ArgumentException>(() => this.webNotificationRequestItem.ToEntity(applicationName));
            Assert.IsTrue(ex.Message.StartsWith("The application name is mandatory.", StringComparison.Ordinal));
        }
    }
}
