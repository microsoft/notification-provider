// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinesLibrary.V1.EmailManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary.Business.V1;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.BusinessLibrary.Providers;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Data;
    using NotificationService.UnitTests.BusinessLibrary.V1.EmailManager;
    using NUnit.Framework;

    /// <summary>
    /// Tests for ProcessEmailNotifications method of Email Manager.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProcessEmailNotificationsTests : EmailManagerTestsBase
    {
        /// <summary>
        /// Initialization for the tests.
        /// </summary>
        [SetUp]
        public void Setup() => this.SetupTestBase();

        /// <summary>
        /// Tests for ProcessEmailNotifications method for valid inputs.
        /// </summary>
        [Test]
        public void ProcessEmailNotificationsTestValidInputWithoutBatch()
        {
            Task<IList<NotificationResponse>> result = this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() } });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method for valid inputs.
        /// </summary>
        [Test]
        public void ProcessEmailNotificationsTestValidInputWithBatch()
        {
            this.MsGraphSetting.Value.EnableBatching = true;
            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() } });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method with overridden From address from Application configuration.
        /// </summary>
        [Test]
        public void ProcessEmailNotificationsTestFromOverride()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = true, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting", JsonConvert.SerializeObject(retrySetting) },
            };

            // this.Configuration = new ConfigurationBuilder()
            //    .AddInMemoryCollection(testConfigValues)
            //    .Build();
            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() } });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method with overridden From address from Application configuration.
        /// </summary>
        /// <returns>The Task.</returns>
        [Test]
        public async Task ProcessEmailNotificationsTest_AlreadyProcessed()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = true, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };
            var notificationId = Guid.NewGuid().ToString();
            var notificationId2 = Guid.NewGuid().ToString();
            IList<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>()
            {
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId,
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId,
                    Status = NotificationItemStatus.Sent,
                },
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId2,
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId2,
                    Status = NotificationItemStatus.Sent,
                },
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting", JsonConvert.SerializeObject(retrySetting) },
            };

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<List<string>>(), It.IsAny<string>()))
                .Returns(Task.FromResult(emailNotificationItemEntities));

            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);
            var result = await this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() }, IgnoreAlreadySent = true }).ConfigureAwait(false);
            Assert.AreEqual(result.Where(x => x.Status == NotificationItemStatus.Sent).Count(), 2);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Never);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method with overridden From address from Application configuration.
        /// </summary>
        [Test]
        public void ProcessEmailNotificationsTest_PartiallyProcessedBatch()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = true, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };
            var notificationId = Guid.NewGuid().ToString();
            var notificationId2 = Guid.NewGuid().ToString();
            IList<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>()
            {
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId,
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId,
                    Status = NotificationItemStatus.Sent,
                },
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId2,
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId2,
                    Status = NotificationItemStatus.Queued,
                },
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting", JsonConvert.SerializeObject(retrySetting) },
            };

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(Task.FromResult(emailNotificationItemEntities));

            _ = this.MsGraphProvider
                .Setup(gp => gp.SendEmailNotification(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<EmailMessagePayload>(), It.IsAny<string>()))
                .Returns(Task.FromResult(this.response));

            _ = this.EmailAccountManager
                .Setup(ema => ema.FetchAccountToBeUsedForApplication(It.IsAny<string>(), It.IsAny<List<ApplicationAccounts>>()))
                .Returns(applicationAccounts[0].Accounts[0]);

            this.MSGraphNotificationProvider = new MSGraphNotificationProvider(this.Configuration, this.EmailAccountManager.Object, this.Logger, this.MsGraphSetting, this.TokenHelper.Object, this.MsGraphProvider.Object, this.EmailManager);

            _ = this.NotificationProviderFactory
                .Setup(provider => provider.GetNotificationProvider(NotificationProviderType.Graph))
                .Returns(this.MSGraphNotificationProvider);

            _ = this.TokenHelper
                .Setup(th => th.GetAuthenticationHeaderValueForSelectedAccount(It.IsAny<AccountCredential>()))
                .ReturnsAsync(new AuthenticationHeaderValue(ApplicationConstants.BearerAuthenticationScheme, "Test"));

            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() }, IgnoreAlreadySent = true });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            Assert.AreEqual(2, result.Result.Where(x => x.Status == NotificationItemStatus.Sent).Count());

            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method with overridden From address from Application configuration.
        /// </summary>
        [Test]
        public void ProcessEmailNotificationsTest_NoActiveAccounts()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = false, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };
            var notificationId = Guid.NewGuid().ToString();
            var notificationId2 = Guid.NewGuid().ToString();
            IList<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>()
            {
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId,
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId,
                    Status = NotificationItemStatus.Sent,
                },
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId2,
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId2,
                    Status = NotificationItemStatus.Queued,
                },
            };
            var mailSettings = new List<MailSettings>()
            {
                new MailSettings()
                {
                    ApplicationName = this.ApplicationName,
                    MailOn = true,
                    SendForReal = false,
                    ToOverride = "user@contoso.com",
                    SaveToSent = true,
                },
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { ConfigConstants.StorageType, StorageType.StorageAccount.ToString() },
                { "MailSettings", JsonConvert.SerializeObject(mailSettings) },
                { ConfigConstants.NotificationProviderType, NotificationProviderType.Graph.ToString() },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(Task.FromResult(emailNotificationItemEntities));

            _ = this.MsGraphProvider
                .Setup(gp => gp.SendEmailNotification(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<EmailMessagePayload>(), It.IsAny<string>()))
                .Returns(Task.FromResult(this.response));

            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() }, IgnoreAlreadySent = true });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            Assert.Pass();
            Assert.AreEqual(1, result.Result.Where(x => x.Status == NotificationItemStatus.Failed).Count());
            Assert.IsTrue(result.Result.FirstOrDefault(x => x.Status == NotificationItemStatus.Failed).ErrorMessage.Contains("No active/valid email account exists for the application"));
            Assert.AreEqual(1, result.Result.Where(x => x.Status == NotificationItemStatus.Sent).Count());

            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method for invalid inputs.
        /// </summary>
        [Test]
        public void ProcessEmailNotificationsTestInvalidInput()
        {
            _ = Assert.ThrowsAsync<ArgumentException>(async () => await this.EmailServiceManager.ProcessEmailNotifications(null, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() } }));
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () => await this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, null));
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method with MailOn Application configuration.
        /// </summary>
        [Test]
        public void ProcessEmailNotificationsTestMailOnFalse()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = false, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };
            var notificationId = Guid.NewGuid().ToString();
            IList<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>()
            {
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId,
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId,
                    Status = NotificationItemStatus.Queued,
                },
            };
            var mailSettings = new List<MailSettings>()
            {
                new MailSettings()
                {
                    ApplicationName = this.ApplicationName,
                    MailOn = false,
                    SendForReal = true,
                },
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { ConfigConstants.StorageType, StorageType.StorageAccount.ToString() },
                { "MailSettings", JsonConvert.SerializeObject(mailSettings) },
                { ConfigConstants.NotificationProviderType, NotificationProviderType.Graph.ToString() },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(Task.FromResult(emailNotificationItemEntities));

            _ = this.MsGraphProvider
                .Setup(gp => gp.SendEmailNotification(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<EmailMessagePayload>(), It.IsAny<string>()))
                .Returns(Task.FromResult(this.response));

            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() }, IgnoreAlreadySent = true });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            Assert.Pass();
            Assert.AreEqual(1, result.Result.Where(x => x.Status == NotificationItemStatus.FakeMail).Count());
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method with MailOn Application configuration.
        /// </summary>
        [Test]
        public void ProcessEmailNotificationsTestSendForReal_MailOnTrue()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = false, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };
            var notificationId = Guid.NewGuid().ToString();
            IList<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>()
            {
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId,
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId,
                    Status = NotificationItemStatus.Queued,
                },
            };
            var mailSettings = new List<MailSettings>()
            {
                new MailSettings()
                {
                    ApplicationName = this.ApplicationName,
                    MailOn = true,
                    SendForReal = true,
                },
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { ConfigConstants.StorageType, StorageType.StorageAccount.ToString() },
                { "MailSettings", JsonConvert.SerializeObject(mailSettings) },
                { ConfigConstants.NotificationProviderType, NotificationProviderType.Graph.ToString() },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(Task.FromResult(emailNotificationItemEntities));

            _ = this.MsGraphProvider
                .Setup(gp => gp.SendEmailNotification(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<EmailMessagePayload>(), It.IsAny<string>()))
                .Returns(Task.FromResult(this.response));

            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() }, IgnoreAlreadySent = true });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            Assert.Pass();
            Assert.AreEqual(1, result.Result.Where(x => x.Status == NotificationItemStatus.Sent).Count());
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method withSendForReal and No ovveride Application configuration.
        /// </summary>
        [Test]
        public void ProcessEmailNotificationsTestSendForReal_NoToOverride()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = false, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };
            var notificationId = Guid.NewGuid().ToString();
            IList<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>()
            {
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId,
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId,
                    Status = NotificationItemStatus.Queued,
                },
            };
            var mailSettings = new List<MailSettings>()
            {
                new MailSettings()
                {
                    ApplicationName = this.ApplicationName,
                    MailOn = true,
                    SendForReal = false,
                },
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { ConfigConstants.StorageType, StorageType.StorageAccount.ToString() },
                { "MailSettings", JsonConvert.SerializeObject(mailSettings) },
                { ConfigConstants.NotificationProviderType, NotificationProviderType.Graph.ToString() },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(Task.FromResult(emailNotificationItemEntities));

            _ = this.MsGraphProvider
                .Setup(gp => gp.SendEmailNotification(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<EmailMessagePayload>(), It.IsAny<string>()))
                .Returns(Task.FromResult(this.response));

            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() }, IgnoreAlreadySent = true });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            Assert.Pass();
            Assert.AreEqual(1, result.Result.Where(x => x.Status == NotificationItemStatus.Failed && x.ErrorMessage.Contains("toOverride cannot be null or empty in case of SendForReal is false")).Count());
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method withSendForReal and To ovveride Application configuration.
        /// </summary>
        [Test]
        public void ProcessEmailNotificationsTestSendForReal_ToOverride()
        {
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
                    FromOverride = "TestFrom",
                    Accounts = new List<AccountCredential>()
                    {
                        new AccountCredential()
                        {
                            AccountName = "Test", IsEnabled = false, PrimaryPassword = "Test",
                        },
                    },
                },
            };

            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };
            var notificationId = Guid.NewGuid().ToString();
            IList<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>()
            {
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId,
                    To = "user@contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId,
                    Status = NotificationItemStatus.Queued,
                },
            };
            var mailSettings = new List<MailSettings>()
            {
                new MailSettings()
                {
                    ApplicationName = this.ApplicationName,
                    MailOn = true,
                    SendForReal = false,
                    ToOverride = "user1@contoso.com",
                },
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { ConfigConstants.StorageType, StorageType.StorageAccount.ToString() },
                { "MailSettings", JsonConvert.SerializeObject(mailSettings) },
                { ConfigConstants.NotificationProviderType, NotificationProviderType.Graph.ToString() },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(Task.FromResult(emailNotificationItemEntities));

            _ = this.MsGraphProvider
                .Setup(gp => gp.SendEmailNotification(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<EmailMessagePayload>(), It.IsAny<string>()))
                .Returns(Task.FromResult(this.response));

            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() }, IgnoreAlreadySent = true });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            Assert.Pass();
            Assert.AreEqual(1, result.Result.Where(x => x.Status == NotificationItemStatus.Sent).Count());
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method using Direct Send configuration.
        /// </summary>
        [Test]
        public void ProcessEmailNotificationsTestSendUsingDirectSend()
        {
            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };
            var notificationId = Guid.NewGuid().ToString();
            IList<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>()
            {
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId,
                    To = "user@contoso.com",
                    From = "user@xxx.contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId,
                    Status = NotificationItemStatus.Queued,
                },
            };
            var mailSettings = new List<MailSettings>()
            {
                new MailSettings()
                {
                    ApplicationName = this.ApplicationName,
                    MailOn = true,
                    SendForReal = true,
                },
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { ConfigConstants.StorageType, StorageType.StorageAccount.ToString() },
                { "MailSettings", JsonConvert.SerializeObject(mailSettings) },
                { ConfigConstants.NotificationProviderType, NotificationProviderType.DirectSend.ToString() },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(Task.FromResult(emailNotificationItemEntities));

            _ = this.NotificationProviderFactory
                .Setup(provider => provider.GetNotificationProvider(NotificationProviderType.DirectSend).ProcessNotificationEntities(this.ApplicationName, emailNotificationItemEntities))
                .Returns(Task.CompletedTask);

            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() }, IgnoreAlreadySent = true });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for ProcessMeetingNotifications method using Direct Send configuration.
        /// </summary>
        [Test]
        public void ProcessMeetingNotificationsTestSendUsingDirectSend()
        {
            var retrySetting = new RetrySetting
            {
                MaxRetries = 10,
                TransientRetryCount = 3,
            };
            var notificationId = Guid.NewGuid().ToString();
            IList<MeetingNotificationItemEntity> meetingNotificationItemEntities = new List<MeetingNotificationItemEntity>()
            {
                new MeetingNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    NotificationId = notificationId,
                    RequiredAttendees = "user@contoso.com",
                    From = "user@xxx.contoso.com",
                    Subject = "TestEmailSubject",
                    Body = "CfDJ8KvR5DP4DK5GqV1jviPzBnsv3onVDZ-ztz-AvRl_6nvVNg86jfmKjgySREDPW9xNrwpKALT5BIFNX6VK3wzKsxc51dbkQjPPG9l7436wQktrAMRadumTpGKNKG1lLlP0FA",
                    Id = notificationId,
                    Status = NotificationItemStatus.Queued,
                },
            };
            var mailSettings = new List<MailSettings>()
            {
                new MailSettings()
                {
                    ApplicationName = this.ApplicationName,
                    MailOn = true,
                    SendForReal = true,
                },
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { ConfigConstants.StorageType, StorageType.StorageAccount.ToString() },
                { "MailSettings", JsonConvert.SerializeObject(mailSettings) },
                { ConfigConstants.NotificationProviderType, NotificationProviderType.DirectSend.ToString() },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).GetMeetingNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(Task.FromResult(meetingNotificationItemEntities));

            _ = this.NotificationProviderFactory
                .Setup(provider => provider.GetNotificationProvider(NotificationProviderType.DirectSend).ProcessMeetingNotificationEntities(this.ApplicationName, meetingNotificationItemEntities))
                .Returns(Task.CompletedTask);

            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            Task<IList<NotificationResponse>> result = this.EmailServiceManager.ProcessEmailNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() }, IgnoreAlreadySent = true });
            Assert.AreEqual(result.Status.ToString(), "RanToCompletion");
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once);
            this.EmailNotificationRepository.Verify(repo => repo.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()), Times.Once);
            Assert.Pass();
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method for valid inputs.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessMeetingNotificationsTestValidInputWithBatch()
        {
            this.MsGraphSetting.Value.EnableBatching = true;
            _ = this.NotificationRepo.Setup(x => x.GetMeetingNotificationItemEntities(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(new List<MeetingNotificationItemEntity> { { new MeetingNotificationItemEntity { NotificationId = "TestNotificationId", RequiredAttendees = "user@contoso.com", Subject = "Test Subject", Start = DateTime.Now, End = DateTime.Now.AddDays(1) } } });
            _ = this.EmailNotificationRepository.Setup(x => x.GetRepository(It.IsAny<StorageType>())).Returns(this.NotificationRepo.Object);

            _ = this.NotificationProviderFactory.Setup(x => x.GetNotificationProvider(It.IsAny<NotificationProviderType>())).Returns(this.NotificationProvider);
            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            var result = await this.EmailServiceManager.ProcessMeetingNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() } });
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.Any(x => x.NotificationId == "TestNotificationId" && x.Status == NotificationItemStatus.Sent));
            this.NotificationRepo.Verify(x => x.UpdateMeetingNotificationItemEntities(It.Is<List<MeetingNotificationItemEntity>>(l => l.Any(q => q.NotificationId == "TestNotificationId" && q.Status == NotificationItemStatus.Sent))), Times.Once);
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method for valid inputs.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessMeetingNotificationsTest_IgnoreAlreadySent()
        {
            this.MsGraphSetting.Value.EnableBatching = true;
            _ = this.NotificationRepo.Setup(x => x.GetMeetingNotificationItemEntities(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(new List<MeetingNotificationItemEntity> { { new MeetingNotificationItemEntity { NotificationId = "TestNotificationId", RequiredAttendees = "user@contoso.com", Subject = "Test Subject", Start = DateTime.Now, End = DateTime.Now.AddDays(1), Status = NotificationItemStatus.Sent } } });
            _ = this.EmailNotificationRepository.Setup(x => x.GetRepository(It.IsAny<StorageType>())).Returns(this.NotificationRepo.Object);

            _ = this.NotificationProviderFactory.Setup(x => x.GetNotificationProvider(It.IsAny<NotificationProviderType>())).Returns(this.NotificationProvider);
            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            var result = await this.EmailServiceManager.ProcessMeetingNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { "TestNotificationId" }, IgnoreAlreadySent = true });
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.Any(x => x.NotificationId == "TestNotificationId" && x.Status == NotificationItemStatus.Sent));
            this.NotificationRepo.Verify(x => x.UpdateMeetingNotificationItemEntities(It.Is<List<MeetingNotificationItemEntity>>(l => l.Any(q => q.NotificationId == "TestNotificationId"))), Times.Never);
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method for valid inputs.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessMeetingNotificationsTest_MailSettingsNull()
        {
            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { ConfigConstants.StorageType, StorageType.StorageAccount.ToString() },
                { "MailSettings", null },
                { ConfigConstants.NotificationProviderType, NotificationProviderType.Graph.ToString() },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();
            _ = this.NotificationRepo.Setup(x => x.GetMeetingNotificationItemEntities(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(new List<MeetingNotificationItemEntity> { { new MeetingNotificationItemEntity { NotificationId = "TestNotificationId", RequiredAttendees = "user@contoso.com", Subject = "Test Subject", Start = DateTime.Now, End = DateTime.Now.AddDays(1), Status = NotificationItemStatus.Queued } } });
            _ = this.EmailNotificationRepository.Setup(x => x.GetRepository(It.IsAny<StorageType>())).Returns(this.NotificationRepo.Object);

            _ = this.NotificationProviderFactory.Setup(x => x.GetNotificationProvider(It.IsAny<NotificationProviderType>())).Returns(this.NotificationProvider);
            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            var result = await this.EmailServiceManager.ProcessMeetingNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { "TestNotificationId" }, IgnoreAlreadySent = true });
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.Any(x => x.NotificationId == "TestNotificationId" && x.Status == NotificationItemStatus.Failed));
            this.NotificationRepo.Verify(x => x.UpdateMeetingNotificationItemEntities(It.Is<List<MeetingNotificationItemEntity>>(l => l.Any(q => q.NotificationId == "TestNotificationId" && q.Status == NotificationItemStatus.Failed))), Times.Once);
        }

        /// <summary>
        /// Tests for ProcessEmailNotifications method for valid inputs.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ProcessMeetingNotifications_MailOff()
        {
            var mailSettings = new List<MailSettings>()
            {
                new MailSettings()
                {
                    ApplicationName = this.ApplicationName,
                    MailOn = false,
                    SendForReal = false,
                    ToOverride = "user@contoso.com",
                    SaveToSent = true,
                },
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { ConfigConstants.StorageType, StorageType.StorageAccount.ToString() },
                { "MailSettings", JsonConvert.SerializeObject(mailSettings) },
                { ConfigConstants.NotificationProviderType, NotificationProviderType.Graph.ToString() },
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            this.MsGraphSetting.Value.EnableBatching = true;
            _ = this.NotificationRepo.Setup(x => x.GetMeetingNotificationItemEntities(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(new List<MeetingNotificationItemEntity> { { new MeetingNotificationItemEntity { NotificationId = "TestNotificationId", RequiredAttendees = "user@contoso.com", Subject = "Test Subject", Start = DateTime.Now, End = DateTime.Now.AddDays(1) } } });
            _ = this.EmailNotificationRepository.Setup(x => x.GetRepository(It.IsAny<StorageType>())).Returns(this.NotificationRepo.Object);

            _ = this.NotificationProviderFactory.Setup(x => x.GetNotificationProvider(It.IsAny<NotificationProviderType>())).Returns(this.NotificationProvider);
            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);

            var result = await this.EmailServiceManager.ProcessMeetingNotifications(this.ApplicationName, new QueueNotificationItem { NotificationIds = new string[] { Guid.NewGuid().ToString() } });
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.Any(x => x.NotificationId == "TestNotificationId" && x.Status == NotificationItemStatus.FakeMail));
            this.NotificationRepo.Verify(x => x.UpdateMeetingNotificationItemEntities(It.Is<List<MeetingNotificationItemEntity>>(l => l.Any(q => q.NotificationId == "TestNotificationId" && q.Status == NotificationItemStatus.FakeMail))), Times.Once);
        }
    }
}
