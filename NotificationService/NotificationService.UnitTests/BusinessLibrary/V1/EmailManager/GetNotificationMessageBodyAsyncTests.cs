// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.V1.EmailManager
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Moq;
    using NotificationService.Contracts;
    using NUnit.Framework;

    /// <summary>
    /// Tests for GetNotificationMessageBodyAsync method of Email Manager.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetNotificationMessageBodyAsyncTests : EmailManagerTestsBase
    {
        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup() => this.SetupTestBase();

        /// <summary>
        /// Tests for GetNotificationMessageBodyAsync method for valid inputs.
        /// </summary>
        [Test]
        public void GetNotificationMessageBodyAsyncTestValidInput()
        {
            var notificationId = Guid.NewGuid().ToString();
            EmailNotificationItemEntity notificationItemEntity = new EmailNotificationItemEntity()
            {
                Application = this.ApplicationName,
                NotificationId = notificationId,
                To = "user@contoso.com",
                Subject = "TestEmailSubject",
                TemplateData = "lt7T9B6LY0XcfudckC73CepDKv/i84fJUKO9QsJzBpI=",
                TemplateName = "Test Template",
                Id = notificationId,
            };
            string mergedTemplate = "Testing Html template";

            var result = this.EmailManager.GetNotificationMessageBodyAsync(this.ApplicationName, notificationItemEntity);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            Assert.AreEqual(result.Result.Content, mergedTemplate);

            this.TemplateManager.Verify(tmgr => tmgr.GetMailTemplate(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            this.TemplateMerge.Verify(tmr => tmr.CreateMailBodyUsingTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for GetNotificationMessageBodyAsync method for invalid inputs.
        /// </summary>
        [Test]
        public void GetNotificationMessageBodyAsyncTestInvalidInput()
        {
            var notificationId = Guid.NewGuid().ToString();
            EmailNotificationItemEntity notificationItemEntity = new EmailNotificationItemEntity()
            {
                Application = this.ApplicationName,
                NotificationId = notificationId,
                To = "user@contoso.com",
                Subject = "TestEmailSubject",
                TemplateData = "lt7T9B6LY0XcfudckC73CepDKv/i84fJUKO9QsJzBpI=",
                TemplateName = "Test Template",
                Id = notificationId,
            };

            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailManager.GetNotificationMessageBodyAsync(this.ApplicationName, (EmailNotificationItemEntity)null));
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailManager.GetNotificationMessageBodyAsync(null, notificationItemEntity));
        }
    }
}
