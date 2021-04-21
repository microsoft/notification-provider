// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.V1.EmailManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Azure.Storage.Queue;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Moq;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Business.V1;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.BusinessLibrary.Models;
    using NotificationService.BusinessLibrary.Providers;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Encryption;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Data;
    using NotificationService.Data.Interfaces;
    using NotificationService.UnitTests.Mocks;

    /// <summary>
    /// Base class for Email Manager class tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EmailManagerTestsBase
    {
        private readonly string sendEmailUrl = "v1/sendEmail";
        private readonly string cloudQueueUri = "https://teststorage.blob.core.windows.net";

        /// <summary>
        /// Gets or sets Graph Provider Mock.
        /// </summary>
        public Mock<IMSGraphProvider> MsGraphProvider { get; set; }

        /// <summary>
        /// Gets or sets Cloud Storage Client Mock.
        /// </summary>
        public Mock<ICloudStorageClient> CloudStorageClient { get; set; }

        /// <summary>
        /// Gets or sets Token Helper Mock.
        /// </summary>
        public Mock<ITokenHelper> TokenHelper { get; set; }

        /// <summary>
        /// Gets or sets Email Notification Repository Mock.
        /// </summary>
        public Mock<IRepositoryFactory> EmailNotificationRepository { get; set; }

        /// <summary>
        /// Gets or sets MSGraphSetting Configuration Mock.
        /// </summary>
        public IOptions<MSGraphSetting> MsGraphSetting { get; set; }

        /// <summary>
        /// Gets or sets Logger.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets Configuration.
        /// </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets Test Application name.
        /// </summary>
        public string ApplicationName => "TestApp";

        /// <summary>
        /// Gets Test User Token value.
        /// </summary>
        public string TestToken => "TokenValue";

        /// <summary>
        /// Gets Email notification items array to be used in tests.
        /// </summary>
        public EmailNotificationItem[] EmailNotificationItems => new EmailNotificationItem[]
            {
                new EmailNotificationItem() { To = "user@contoso.com", Subject = "TestSubject", Body = "TestBody" },
            };

        /// <summary>
        /// Gets or sets Instance of Email Manager.
        /// </summary>
        public EmailManager EmailManager { get; set; }

        /// <summary>
        /// Gets or sets Encryption Service Mock.
        /// </summary>
        public Mock<IEncryptionService> EncryptionService { get; set; }

        /// <summary>
        /// Gets or sets EmailAccountManager Service Mock.
        /// </summary>
        public Mock<IEmailAccountManager> EmailAccountManager { get; set; }

        /// <summary>
        /// Gets or sets TemplateManager Mock.
        /// </summary>
        public Mock<IMailTemplateManager> TemplateManager { get; set; }

        /// <summary>
        /// Gets or sets TemplateMerge Mock.
        /// </summary>
        public Mock<ITemplateMerge> TemplateMerge { get; set; }

        /// <summary>
        /// Gets or sets Instance of Email Handler Manager.
        /// </summary>
        public EmailHandlerManager EmailHandlerManager { get; set; }

        /// <summary>
        /// Gets or sets Instance of Email Handler Manager.
        /// </summary>
        public EmailServiceManager EmailServiceManager { get; set; }

        /// <summary>
        /// Gets or sets INotificationProvider Mock.
        /// </summary>
        public Mock<INotificationProviderFactory> NotificationProviderFactory { get; set; }

        /// <summary>
        /// Gets or sets the notification repo.
        /// </summary>
        /// <value>
        /// The notification repo.
        /// </value>
        public Mock<IEmailNotificationRepository> NotificationRepo { get; set; }

        /// <summary>
        /// Gets or sets the notification repo.
        /// </summary>
        /// <value>
        /// The notification repo.
        /// </value>
        public INotificationProvider NotificationProvider { get; set; }

        /// <summary>
        /// Gets or sets the ms graph notification provider.
        /// </summary>
        /// <value>
        /// The ms graph notification provider.
        /// </value>
        public MSGraphNotificationProvider MSGraphNotificationProvider { get; set; }

        /// <summary>
        /// Mocked response data.
        /// </summary>
#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1401 // Fields should be private
        protected ResponseData<string> response;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore SA1201 // Elements should appear in the correct order

        /// <summary>
        /// Initialization for all Email Manager Tests.
        /// </summary>
        protected void SetupTestBase()
        {
            this.MsGraphProvider = new Mock<IMSGraphProvider>();
            this.CloudStorageClient = new Mock<ICloudStorageClient>();
            this.TokenHelper = new Mock<ITokenHelper>();
            this.EmailNotificationRepository = new Mock<IRepositoryFactory>();
            this.MsGraphSetting = Options.Create(new MSGraphSetting() { EnableBatching = false, SendMailUrl = this.sendEmailUrl, BatchRequestLimit = 4 });
            this.Logger = new Mock<ILogger>().Object;
            this.EncryptionService = new Mock<IEncryptionService>();
            this.EmailAccountManager = new Mock<IEmailAccountManager>();
            this.TemplateManager = new Mock<IMailTemplateManager>();
            this.TemplateMerge = new Mock<ITemplateMerge>();
            this.NotificationProviderFactory = new Mock<INotificationProviderFactory>();
            this.NotificationRepo = new Mock<IEmailNotificationRepository>();
            this.NotificationProvider = new MockNotificationProvider();

            var notificationId = Guid.NewGuid().ToString();
            IList<NotificationBatchItemResponse> responses = new List<NotificationBatchItemResponse>();
            responses.Add(new NotificationBatchItemResponse() { NotificationId = notificationId, Status = System.Net.HttpStatusCode.Accepted });
            var applicationAccounts = new List<ApplicationAccounts>()
            {
                new ApplicationAccounts()
                {
                    ApplicationName = this.ApplicationName,
                    ValidAppIds = Guid.NewGuid().ToString(),
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

            var storageAccountSettings = new StorageAccountSetting()
            {
                NotificationQueueName = "test-queue",
            };

            Dictionary<string, string> testConfigValues = new Dictionary<string, string>()
            {
                { "ApplicationAccounts", JsonConvert.SerializeObject(applicationAccounts) },
                { "RetrySetting:MaxRetries", "10" },
                { "RetrySetting:TransientRetryCount", "3" },
                { ConfigConstants.StorageType, StorageType.StorageAccount.ToString() },
                { "MailSettings", JsonConvert.SerializeObject(mailSettings) },
                { ConfigConstants.StorageAccountConfigSectionKey, JsonConvert.SerializeObject(storageAccountSettings) },
                { ConfigConstants.NotificationProviderType, NotificationProviderType.Graph.ToString() },
            };

            string mergedTemplate = "Testing Html template";

            MailTemplate template = new MailTemplate()
            {
                TemplateId = "TestTemplate",
                Description = "Test template",
                Content = "Testing {{Key}} template",
                TemplateType = "Text",
            };

            this.response = new ResponseData<string>()
            {
                Status = true,
                Result = "successful",
                StatusCode = HttpStatusCode.OK,
            };

            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfigValues)
                .Build();

            _ = this.TokenHelper
                .Setup(th => th.GetAccessTokenForSelectedAccount(It.IsAny<AccountCredential>()))
                .Returns(Task.FromResult(this.TestToken));

            _ = this.TokenHelper
                .Setup(th => th.GetAuthenticationHeaderFromToken(It.IsAny<string>()))
                .Returns(Task.FromResult(new System.Net.Http.Headers.AuthenticationHeaderValue(ApplicationConstants.BearerAuthenticationScheme, this.TestToken)));

            _ = this.MsGraphProvider
                .Setup(gp => gp.ProcessEmailRequestBatch(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<GraphBatchRequest>()))
                .Returns(Task.FromResult(responses));

            _ = this.MsGraphProvider
                .Setup(gp => gp.SendEmailNotification(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<EmailMessagePayload>(), It.IsAny<string>()))
                .Returns(Task.FromResult(this.response));

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).CreateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).UpdateEmailNotificationItemEntities(It.IsAny<IList<EmailNotificationItemEntity>>()))
                .Returns(Task.CompletedTask);

            _ = this.EmailNotificationRepository
                .Setup(repository => repository.GetRepository(StorageType.StorageAccount).GetEmailNotificationItemEntities(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(Task.FromResult(emailNotificationItemEntities));

            _ = this.CloudStorageClient
                .Setup(csa => csa.GetCloudQueue(It.IsAny<string>()))
                .Returns(new CloudQueue(new Uri(this.cloudQueueUri)));

            _ = this.CloudStorageClient
                .Setup(csa => csa.QueueCloudMessages(It.IsAny<CloudQueue>(), It.IsAny<IEnumerable<string>>(), null))
                .Returns(Task.CompletedTask);

            _ = this.EmailAccountManager
                .Setup(ema => ema.FetchAccountToBeUsedForApplication(It.IsAny<string>(), It.IsAny<List<ApplicationAccounts>>()))
                .Returns(applicationAccounts[0].Accounts[0]);

            _ = this.TemplateManager
                .Setup(tmgr => tmgr.GetMailTemplate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(template));

            _ = this.TemplateMerge
                .Setup(tmr => tmr.CreateMailBodyUsingTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mergedTemplate);
            this.EmailManager = new EmailManager(this.Configuration, this.EmailNotificationRepository.Object, this.Logger, this.TemplateManager.Object, this.TemplateMerge.Object);

            this.MSGraphNotificationProvider = new MSGraphNotificationProvider(
                this.Configuration,
                this.EmailAccountManager.Object,
                this.Logger,
                this.MsGraphSetting,
                this.TokenHelper.Object,
                this.MsGraphProvider.Object,
                this.EmailManager);

            _ = this.NotificationProviderFactory
                .Setup(provider => provider.GetNotificationProvider(NotificationProviderType.Graph))
                .Returns(this.MSGraphNotificationProvider);

            this.EmailHandlerManager = new EmailHandlerManager(this.Configuration, this.MsGraphSetting, this.CloudStorageClient.Object, this.Logger, this.EmailManager);
            this.EmailServiceManager = new EmailServiceManager(this.Configuration, this.EmailNotificationRepository.Object, this.CloudStorageClient.Object, this.Logger, this.NotificationProviderFactory.Object, this.EmailManager);
        }
    }
}
