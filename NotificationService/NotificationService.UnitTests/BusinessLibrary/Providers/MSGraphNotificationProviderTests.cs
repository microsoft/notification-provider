// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.BusinessLibrary.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Moq;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.BusinessLibrary.Providers;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NUnit.Framework;

    /// <summary>
    /// MSGraphNotificationProviderTests.
    /// </summary>
    public class MSGraphNotificationProviderTests
    {
        private readonly Mock<IConfiguration> configuration;
        private readonly Mock<IEmailAccountManager> emailAccountManager;
        private readonly Mock<ILogger> logger;
        private readonly Mock<IOptions<MSGraphSetting>> mSGraphSetting;
        private readonly Mock<IOptions<RetrySetting>> pollyRetrySetting;
        private readonly Mock<ITokenHelper> tokenHelper;
        private readonly Mock<IMSGraphProvider> msGraphProvider;
        private readonly Mock<IEmailManager> emailManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSGraphNotificationProviderTests"/> class.
        /// </summary>
        public MSGraphNotificationProviderTests()
        {
            this.configuration = new Mock<IConfiguration>();
            this.emailAccountManager = new Mock<IEmailAccountManager>();
            this.logger = new Mock<ILogger>();
            this.mSGraphSetting = new Mock<IOptions<MSGraphSetting>>();
            this.msGraphProvider = new Mock<IMSGraphProvider>();
            this.pollyRetrySetting = new Mock<IOptions<RetrySetting>>();
            this.tokenHelper = new Mock<ITokenHelper>();
            this.emailManager = new Mock<IEmailManager>();
        }

        /// <summary>
        /// ProcessNotificationEntities_Tests.
        /// </summary>
        /// <returns>A Task.</returns>
        [Test]
        public async Task ProcessNotificationEntities_Tests()
        {
            _ = this.configuration.Setup(x => x["ApplicationAccounts"]).Returns(JsonConvert.SerializeObject(new List<ApplicationAccounts> { new ApplicationAccounts { Accounts = new List<AccountCredential> { new AccountCredential { AccountName = "Test", IsEnabled = true } }, ApplicationName = "TestAppName", } }));
            _ = this.configuration.Setup(x => x["RetrySetting:MaxRetries"]).Returns("2");
            _ = this.configuration.Setup(x => x["MailSettings"]).Returns(JsonConvert.SerializeObject(new List<MailSettings> { new MailSettings { ApplicationName = "TestAppName", SendForReal = true } }));
            _ = this.mSGraphSetting.Setup(x => x.Value).Returns(new MSGraphSetting { EnableBatching = true, SendMailUrl = "test", BatchRequestLimit = 4 });
            _ = this.msGraphProvider.Setup(x => x.ProcessEmailRequestBatch(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<GraphBatchRequest>())).ReturnsAsync(new List<NotificationBatchItemResponse> { new NotificationBatchItemResponse { Status = System.Net.HttpStatusCode.Accepted } });
            var provider = new MSGraphNotificationProvider(this.configuration.Object, new EmailAccountManager(), this.logger.Object, this.mSGraphSetting.Object, this.tokenHelper.Object, this.msGraphProvider.Object, this.emailManager.Object);
            var notifications = new List<EmailNotificationItemEntity> { new EmailNotificationItemEntity { Application = "TestAppName", SendOnUtcDate = DateTime.Parse("2021-01-18T12:00:00Z"), To = "test@microsoft.com" } };
            await provider.ProcessNotificationEntities("TestAppName", notifications);
            Assert.IsTrue(notifications.Count == 1);
            Assert.IsTrue(notifications.Any(x => x.Status == NotificationItemStatus.Failed));

            _ = this.tokenHelper.Setup(x => x.GetAuthenticationHeaderValueForSelectedAccount(It.IsAny<AccountCredential>())).ReturnsAsync(new AuthenticationHeaderValue("Test"));
            provider = new MSGraphNotificationProvider(this.configuration.Object, new EmailAccountManager(), this.logger.Object, this.mSGraphSetting.Object, this.tokenHelper.Object, this.msGraphProvider.Object, this.emailManager.Object);
            await provider.ProcessNotificationEntities("TestAppName", notifications);
            Assert.IsTrue(notifications.Count == 1);
            Assert.IsTrue(notifications.Any(x => x.Status == NotificationItemStatus.Sent));
            this.msGraphProvider.Verify(x => x.ProcessEmailRequestBatch(It.IsAny<AuthenticationHeaderValue>(), It.Is<GraphBatchRequest>(b => b.Requests.Any(g => ((NotificationService.Contracts.EmailMessagePayload)g.Body).Message.SingleValueExtendedProperties[0].Id.Contains("SystemTime 0x3FEF")))), Times.Once);
            this.msGraphProvider.Verify(x => x.ProcessEmailRequestBatch(It.IsAny<AuthenticationHeaderValue>(), It.Is<GraphBatchRequest>(b => b.Requests.Any(g => ((NotificationService.Contracts.EmailMessagePayload)g.Body).Message.SingleValueExtendedProperties[0].Value.Contains("2021-01-18")))), Times.Once);
        }
    }
}
