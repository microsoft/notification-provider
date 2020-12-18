// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Data.Repositories
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using NotificationService.Common;
    using NotificationService.Contracts;
    using NotificationService.Data;

    /// <summary>
    /// Base class for Email Notification Repository class tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EmailNotificationRepositoryTestsBase
    {
        /// <summary>
        /// Gets or sets CosmosDBSetting Configuration Mock.
        /// </summary>
        public IOptions<CosmosDBSetting> CosmosDBSetting { get; set; }

        /// <summary>
        /// Gets or sets Logger.
        /// </summary>
        public ILogger<EmailNotificationRepository> Logger { get; set; }

        /// <summary>
        /// Gets or sets Cosmos Linq Query Mock.
        /// </summary>
        public Mock<ICosmosLinqQuery> CosmosLinqQuery { get; set; }

        /// <summary>
        /// Gets or sets Cosmos DB Query Client Mock.
        /// </summary>
        public Mock<ICosmosDBQueryClient> CosmosDBQueryClient { get; set; }

        /// <summary>
        /// Gets or sets Cosmos Container Mock.
        /// </summary>
        public Mock<Container> CosmosContainer { get; set; }

        /// <summary>
        /// Gets or sets Email Notification Repository instance.
        /// </summary>
        public EmailNotificationRepository EmailNotificationRepository { get; set; }

        /// <summary>
        /// GEts or sets Mail Attachment Reporisotry instance.
        /// </summary>
        public Mock<IMailAttachmentRepository> MailAttachmentRepository { get; set; }

        /// <summary>
        /// Gets Test Application name.
        /// </summary>
        public string ApplicationName
        {
            get => "TestApp";
        }

        /// <summary>
        /// Gets test notification entities.
        /// </summary>
        public IList<EmailNotificationItemEntity> NotificationEntities
        {
            get => new List<EmailNotificationItemEntity>()
            {
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    Id = "TestId",
                    To = "user@contoso.com",
                    Subject = "TestSubject",
                    Body = "TestBody",
                },
                new EmailNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    Id = "TestId2",
                    To = "user@contoso.com",
                    Subject = "TestSubject",
                    Body = "TestBody",
                },
            };
        }

        /// <summary>
        /// Initialization for all Email Manager Tests.
        /// </summary>
        protected void SetupTestBase()
        {
            this.CosmosLinqQuery = new Mock<ICosmosLinqQuery>();
            this.CosmosDBQueryClient = new Mock<ICosmosDBQueryClient>();
            this.CosmosContainer = new Mock<Container>();
            this.MailAttachmentRepository = new Mock<IMailAttachmentRepository>();
            var mockItemResponse = new Mock<ItemResponse<EmailNotificationItemEntity>>();
            var mockFeedIterator = new Mock<FeedIterator<EmailNotificationItemEntity>>();

            IOrderedQueryable<EmailNotificationItemEntity> queryableEntityReponse = this.NotificationEntities.AsQueryable().OrderBy(e => e.NotificationId);
            this.CosmosDBSetting = Options.Create(new CosmosDBSetting() { Database = "TestDatabase", Container = "TestContainer", Key = "TestKey", Uri = "TestUri" });
            this.Logger = Mock.Of<ILogger<EmailNotificationRepository>>();

            IQueryable<EmailNotificationItemEntity> queryResult = null;
            _ = this.CosmosLinqQuery
                .Setup(clq => clq.GetFeedIterator(It.IsAny<IQueryable<EmailNotificationItemEntity>>()))
                .Callback<IQueryable<EmailNotificationItemEntity>>(r => queryResult = r)
                .Returns(mockFeedIterator.Object);

            _ = this.CosmosContainer
                .Setup(container => container.CreateItemAsync(It.IsAny<EmailNotificationItemEntity>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockItemResponse.Object));

            _ = this.CosmosContainer
                .Setup(container => container.UpsertItemAsync(It.IsAny<EmailNotificationItemEntity>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockItemResponse.Object));

            _ = this.CosmosContainer
                .Setup(container => container.GetItemLinqQueryable<EmailNotificationItemEntity>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(queryableEntityReponse);

            _ = this.CosmosDBQueryClient
                .Setup(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(this.CosmosContainer.Object);

            this.EmailNotificationRepository = new EmailNotificationRepository(this.CosmosDBSetting, this.CosmosDBQueryClient.Object, this.Logger, this.CosmosLinqQuery.Object, this.MailAttachmentRepository.Object);
        }
    }
}
