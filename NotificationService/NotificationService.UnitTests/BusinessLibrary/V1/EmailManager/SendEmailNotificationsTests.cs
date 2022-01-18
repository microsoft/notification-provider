// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinesLibrary.V1.EmailManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Azure.Storage.Queue;
    using Moq;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Business.V1;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.BusinessLibrary.Providers;
    using NotificationService.Common;
    using NotificationService.Contracts;
    using NotificationService.Data;
    using NotificationService.UnitTests.BusinessLibrary.V1.EmailManager;
    using NUnit.Framework;

    /// <summary>
    /// Tests for SendEmailNotifications method of Email Controller.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SendEmailNotificationsTests : EmailManagerTestsBase
    {
        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup() => this.SetupTestBase();

        /// <summary>
        /// Tests for SendEmailNotifications method for valid inputs.
        /// </summary>
        [Test]
        public void SendEmailNotificationsTestValidInputWithoutBatch()
        {
            Task<IList<NotificationResponse>> result = this.EmailServiceManager.SendEmailNotifications(this.ApplicationName, this.EmailNotificationItems);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).CreateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for SendEmailNotifications method for valid inputs with batching enabled.
        /// </summary>
        [Test]
        public void SendEmailNotificationsTestValidInputWithBatch()
        {
            this.MsGraphSetting.Value.EnableBatching = true;
            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = this.EmailServiceManager.SendEmailNotifications(this.ApplicationName, this.EmailNotificationItems);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).CreateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for SendEmailNotifications method for invalid inputs.
        /// </summary>
        [Test]
        public void SendEmailNotificationsTestInvalidInput()
        {
            _ = Assert.ThrowsAsync<ArgumentException>(async () => await this.EmailServiceManager.SendEmailNotifications(null, this.EmailNotificationItems));
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailServiceManager.SendEmailNotifications(this.ApplicationName, null));
        }

        /// <summary>
        /// Tests for SendEmailNotifications method mocking Transient Errors from Graph and testing Requeue.
        /// </summary>
        [Test]
        public void SendEmailNotificationsTestRequeueForTransientErrors()
        {
            this.MsGraphSetting.Value.EnableBatching = false;
            EmailNotificationItem[] notificationItems = new EmailNotificationItem[]
            {
                new EmailNotificationItem() { To = "user@contoso.com", Subject = "TestSubject", Body = "TestBody", NotificationId = "1" },
                new EmailNotificationItem() { To = "user@contoso.com", Subject = "TestSubject", Body = "TestBody", NotificationId = "2" },
            };

            IList<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>()
            {
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = "1",
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = "1",
                },
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = "2",
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = "2",
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(Task.FromResult(emailNotificationItemEntities));

            // Test the transient error: Too many Requests/ Request Timeout
            var graphProvider = new Mock<IMSGraphProvider>();

            this.response.Status = false;
            this.response.StatusCode = HttpStatusCode.TooManyRequests;
            _ = graphProvider
                .Setup(gp => gp.SendEmailNotification(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<EmailMessagePayload>(), It.IsAny<string>()))
                .Returns(Task.FromResult(this.response));

            _ = this.TokenHelper
                .Setup(th => th.GetAuthenticationHeaderValueForSelectedAccount(It.IsAny<AccountCredential>()))
                .ReturnsAsync(new AuthenticationHeaderValue(ApplicationConstants.BearerAuthenticationScheme, "Test"));

            this.MSGraphNotificationProvider = new MSGraphNotificationProvider(this.Configuration, this.EmailAccountManager.Object, this.Logger, this.MsGraphSetting, this.TokenHelper.Object, graphProvider.Object, this.EmailManager);

            _ = this.NotificationProviderFactory
                .Setup(provider => provider.GetNotificationProvider(NotificationProviderType.Graph))
                .Returns(this.MSGraphNotificationProvider);

            var emailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = emailServiceManager.SendEmailNotifications(this.ApplicationName, notificationItems);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            Assert.AreEqual(result.Result.Count(x => x.Status == NotificationItemStatus.Retrying), notificationItems.Count());
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            this.CloudStorageClient.Verify(csa => csa.QueueCloudMessages(It.IsAny<CloudQueue>(), It.IsAny<IEnumerable<string>>(), null), Times.Once);

            // When Graph calls succeed
            this.response.Status = true;
            this.response.StatusCode = HttpStatusCode.OK;
            _ = graphProvider
                .Setup(gp => gp.SendEmailNotification(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<EmailMessagePayload>(), It.IsAny<string>()))
                .Returns(Task.FromResult(this.response));

            emailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            result = emailServiceManager.SendEmailNotifications(this.ApplicationName, notificationItems);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            Assert.AreEqual(result.Result.Count(x => x.Status == NotificationItemStatus.Sent), notificationItems.Count());

            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Exactly(2));

            // This is called when retrying the transient failed items previously, count not changed
            this.CloudStorageClient.Verify(csa => csa.QueueCloudMessages(It.IsAny<CloudQueue>(), It.IsAny<IEnumerable<string>>(), null), Times.Once);

            // When graph call throws exception
            _ = graphProvider
                .Setup(gp => gp.SendEmailNotification(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<EmailMessagePayload>(), It.IsAny<string>()))
                .ThrowsAsync(new AggregateException());

            emailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            result = emailServiceManager.SendEmailNotifications(this.ApplicationName, notificationItems);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            Assert.AreEqual(result.Result.Count(x => x.Status == NotificationItemStatus.Failed), notificationItems.Count());

            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Exactly(3));

            // This is called when retrying the transient failed items previously, count not changed
            this.CloudStorageClient.Verify(csa => csa.QueueCloudMessages(It.IsAny<CloudQueue>(), It.IsAny<IEnumerable<string>>(), null), Times.Once);

            Assert.Pass();
        }

        /// <summary>
        /// Tests for Send Notifications In Batch method mocking Transient Errors from Graph and testing Requeue.
        /// </summary>
        [Test]
        public void SendEmailNotificationsInBatchTestRequeueForTransientErrors()
        {
            this.MsGraphSetting.Value.EnableBatching = true;
            EmailNotificationItem[] notificationItems = new EmailNotificationItem[]
            {
                new EmailNotificationItem() { To = "user@contoso.com", Subject = "TestSubject", Body = "TestBody", NotificationId = "1" },
                new EmailNotificationItem() { To = "user@contoso.com", Subject = "TestSubject", Body = "TestBody", NotificationId = "2" },
                new EmailNotificationItem() { To = "user@contoso.com", Subject = "TestSubject", Body = "TestBody", NotificationId = "3" },
            };

            IList<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>()
            {
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = "1",
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = "1",
                },
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = "2",
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = "2",
                },
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = "3",
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = "3",
                },
            };
            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };

            // Test the transient error: Too many Requests/ Request Timeout
            IList<NotificationBatchItemResponse> responses = new List<NotificationBatchItemResponse>();
            responses.Add(new NotificationBatchItemResponse() { NotificationId = "1", Status = System.Net.HttpStatusCode.TooManyRequests });
            responses.Add(new NotificationBatchItemResponse() { NotificationId = "2", Status = System.Net.HttpStatusCode.RequestTimeout });
            responses.Add(new NotificationBatchItemResponse() { NotificationId = "3", Status = System.Net.HttpStatusCode.Accepted });

            Mock<IMSGraphProvider> graphProvider = new Mock<IMSGraphProvider>();
            _ = graphProvider
                .Setup(gp => gp.ProcessEmailRequestBatch(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<GraphBatchRequest>()))
                .Returns(Task.FromResult(responses));

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(Task.FromResult(emailNotificationItemEntities));

            _ = this.TokenHelper
                .Setup(th => th.GetAuthenticationHeaderValueForSelectedAccount(It.IsAny<AccountCredential>()))
                .ReturnsAsync(new AuthenticationHeaderValue(ApplicationConstants.BearerAuthenticationScheme, "Test"));

            this.MSGraphNotificationProvider = new MSGraphNotificationProvider(this.Configuration, this.EmailAccountManager.Object, this.Logger, this.MsGraphSetting, this.TokenHelper.Object, graphProvider.Object, this.EmailManager);

            _ = this.NotificationProviderFactory
                .Setup(provider => provider.GetNotificationProvider(NotificationProviderType.Graph))
                .Returns(this.MSGraphNotificationProvider);

            var emailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = emailServiceManager.SendEmailNotifications(this.ApplicationName, notificationItems);
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            Assert.AreEqual(result.Result.Count(x => x.Status == NotificationItemStatus.Retrying), 2);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            this.CloudStorageClient.Verify(csa => csa.QueueCloudMessages(It.IsAny<CloudQueue>(), It.IsAny<IEnumerable<string>>(), null), Times.Once);
            Assert.Pass();
        }
    }
}
