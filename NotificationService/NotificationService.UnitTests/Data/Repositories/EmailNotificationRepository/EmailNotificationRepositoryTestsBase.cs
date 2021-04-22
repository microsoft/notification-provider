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
    using Microsoft.Extensions.Options;
    using Moq;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Extensions;
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
        public ILogger Logger { get; set; }

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
        public Mock<Container> EmailHistoryContainer { get; set; }

        /// <summary>
        /// Gets or sets Cosmos Container Mock.
        /// </summary>
        public Mock<Container> MeetingHistoryContainer { get; set; }

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
        /// Gets test Meeting notification entities.
        /// </summary>
        public IList<MeetingNotificationItemEntity> meetingNotificationEntities
        {
            get => new List<MeetingNotificationItemEntity>()
            {
                new MeetingNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    Id = "TestId",
                    RequiredAttendees = "user@contoso.com",
                    Subject = "TestSubject",
                    Body = "TestBody",
                },
                new MeetingNotificationItemEntity()
                {
                    Application = this.ApplicationName,
                    Id = "TestId2",
                    RequiredAttendees = "user@contoso.com",
                    Subject = "TestSubject",
                    Body = "TestBody",
                },
            };
        }

        /// <summary>
        /// Gets MailHistoryContainerName.
        /// </summary>
        protected string MailHistoryContainerName { get => "TestEmailContainer"; }

        /// <summary>
        /// Gets MeetingHistoryContainerName.
        /// </summary>
        protected string MeetingHistoryContainerName { get => "TestMeetingContainer"; }

        /// <summary>
        /// Initialization for all Email Manager Tests.
        /// </summary>
        protected void SetupTestBase()
        {
            this.CosmosLinqQuery = new Mock<ICosmosLinqQuery>();
            this.CosmosDBQueryClient = new Mock<ICosmosDBQueryClient>();
            this.EmailHistoryContainer = new Mock<Container>();
            this.MeetingHistoryContainer = new Mock<Container>();
            this.MailAttachmentRepository = new Mock<IMailAttachmentRepository>();
            var mockEmailItemResponse = new Mock<ItemResponse<EmailNotificationItemCosmosDbEntity>>();
            var mockEmailFeedIterator = new Mock<FeedIterator<EmailNotificationItemCosmosDbEntity>>();
            var mockMeetingItemResponse = new Mock<ItemResponse<MeetingNotificationItemCosmosDbEntity>>();
            var mockMeetingFeedIterator = new Mock<FeedIterator<MeetingNotificationItemCosmosDbEntity>>();

            List<EmailNotificationItemCosmosDbEntity> emailNotificationItemCosmosDbEntities = new List<EmailNotificationItemCosmosDbEntity>();
            foreach (var item in this.NotificationEntities)
            {
                emailNotificationItemCosmosDbEntities.Add(item.ConvertToEmailNotificationItemCosmosDbEntity());
            }

            List<MeetingNotificationItemCosmosDbEntity> meetingNotificationItemCosmosDbEntities = new List<MeetingNotificationItemCosmosDbEntity>();
            foreach (var item in this.meetingNotificationEntities)
            {
                meetingNotificationItemCosmosDbEntities.Add(item.ConvertToMeetingNotificationItemCosmosDbEntity());
            }

            IOrderedQueryable<EmailNotificationItemCosmosDbEntity> queryableEmailEntityReponse = emailNotificationItemCosmosDbEntities.AsQueryable().OrderBy(e => e.NotificationId);

            IOrderedQueryable<MeetingNotificationItemCosmosDbEntity> queryableMeetingEntityReponse = meetingNotificationItemCosmosDbEntities.AsQueryable().OrderBy(e => e.NotificationId);

            this.CosmosDBSetting = Options.Create(new CosmosDBSetting() { Database = "TestDatabase", EmailHistoryContainer = this.MailHistoryContainerName, MeetingHistoryContainer = "TestMeetingContainer", Key = "TestKey", Uri = "TestUri" });
            this.Logger = Mock.Of<ILogger>();

            IQueryable<EmailNotificationItemCosmosDbEntity> queryResult = null;
            _ = this.CosmosLinqQuery
                .Setup(clq => clq.GetFeedIterator(It.IsAny<IQueryable<EmailNotificationItemCosmosDbEntity>>()))
                .Callback<IQueryable<EmailNotificationItemCosmosDbEntity>>(r => queryResult = r)
                .Returns(mockEmailFeedIterator.Object);

            _ = this.EmailHistoryContainer
                .Setup(container => container.CreateItemAsync(It.IsAny<EmailNotificationItemCosmosDbEntity>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockEmailItemResponse.Object));

            _ = this.EmailHistoryContainer
                .Setup(container => container.UpsertItemAsync(It.IsAny<EmailNotificationItemCosmosDbEntity>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockEmailItemResponse.Object));

            _ = this.EmailHistoryContainer
                .Setup(container => container.GetItemLinqQueryable<EmailNotificationItemCosmosDbEntity>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(queryableEmailEntityReponse);

            _ = this.CosmosDBQueryClient
                .Setup(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), this.MailHistoryContainerName))
                .Returns(this.EmailHistoryContainer.Object);


            IQueryable<MeetingNotificationItemCosmosDbEntity> meetingQueryResult = null;
            _ = this.CosmosLinqQuery
                .Setup(clq => clq.GetFeedIterator(It.IsAny<IQueryable<MeetingNotificationItemCosmosDbEntity>>()))
                .Callback<IQueryable<MeetingNotificationItemCosmosDbEntity>>(r => meetingQueryResult = r)
                .Returns(mockMeetingFeedIterator.Object);

            _ = this.MeetingHistoryContainer
                .Setup(container => container.CreateItemAsync(It.IsAny<MeetingNotificationItemCosmosDbEntity>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockMeetingItemResponse.Object));

            _ = this.MeetingHistoryContainer
                .Setup(container => container.UpsertItemAsync(It.IsAny<MeetingNotificationItemCosmosDbEntity>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockMeetingItemResponse.Object));

            _ = this.MeetingHistoryContainer
                .Setup(container => container.GetItemLinqQueryable<MeetingNotificationItemCosmosDbEntity>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(queryableMeetingEntityReponse);

            _ = this.CosmosDBQueryClient
                .Setup(cdq => cdq.GetCosmosContainer(It.IsAny<string>(), this.MeetingHistoryContainerName))
                .Returns(this.MeetingHistoryContainer.Object);


            this.EmailNotificationRepository = new EmailNotificationRepository(this.CosmosDBSetting, this.CosmosDBQueryClient.Object, this.Logger, this.CosmosLinqQuery.Object, this.MailAttachmentRepository.Object);
        }
    }
}
