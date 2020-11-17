// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NotificationService.Common;
    using NotificationService.Common.Exceptions;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Entities.Web;
    using NotificationService.Data.Interfaces;

    /// <summary>
    /// The <see cref="WebNotificationItemEntityRepository"/> class manages the web notification entity persistence and projection.
    /// </summary>
    /// <remarks>The repository treats patitionkey = entityId for parameter references.</remarks>
    /// <seealso cref="IRepository{T}" />
    public class WebNotificationItemEntityRepository : IRepository<WebNotificationItemEntity>
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<WebNotificationItemEntityRepository> logger;

        /// <summary>
        /// The Cosmos container.
        /// </summary>
        private readonly Container cosmosContainer;

        /// <summary>
        /// The Cosmos database setting.
        /// </summary>
        private readonly CosmosDBSetting cosmosDBSetting;

        /// <summary>
        /// The Cosmos database query client.
        /// </summary>
        private readonly ICosmosDBQueryClient cosmosDBQueryClient;

        /// <summary>
        /// The cosmos linq query.
        /// </summary>
        private readonly ICosmosLinqQuery cosmosLinqQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebNotificationItemEntityRepository"/> class.
        /// </summary>
        /// <param name="cosmosDBSetting">The instance for <see cref="IOptions{CosmosDBSetting}"/>.</param>
        /// <param name="cosmosDBQueryClient">The instance for <see cref="ICosmosDBQueryClient"/>.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="cosmosLinqQuery">The instance for <see cref="ICosmosLinqQuery"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// cosmosDBSetting
        /// or
        /// cosmosDBQueryClient
        /// or
        /// logger.
        /// </exception>
        public WebNotificationItemEntityRepository(IOptions<CosmosDBSetting> cosmosDBSetting, ICosmosDBQueryClient cosmosDBQueryClient, ILogger<WebNotificationItemEntityRepository> logger, ICosmosLinqQuery cosmosLinqQuery)
        {
            this.cosmosDBSetting = cosmosDBSetting?.Value ?? throw new ArgumentNullException(nameof(cosmosDBSetting));
            this.cosmosDBQueryClient = cosmosDBQueryClient ?? throw new ArgumentNullException(nameof(cosmosDBQueryClient));
            this.cosmosLinqQuery = cosmosLinqQuery ?? throw new ArgumentNullException(nameof(cosmosLinqQuery));
            this.cosmosContainer = this.cosmosDBQueryClient.GetCosmosContainer(this.cosmosDBSetting.Database, this.cosmosDBSetting.Container);
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string entityId)
        {
            bool isSuccess = false;
            this.logger.LogInformation($"Started {nameof(this.DeleteAsync)} method of {nameof(WebNotificationItemEntityRepository)}.");
            if (string.IsNullOrWhiteSpace(entityId))
            {
                throw new ArgumentException("The entity Id is not specified.", nameof(entityId));
            }

            WebNotificationItemEntity webNotificationItemEntity = await this.LoadEntity(entityId).ConfigureAwait(false);
            PartitionKey partKey = new PartitionKey(entityId);
            ItemResponse<WebNotificationItemEntity> response = await this.cosmosContainer.DeleteItemAsync<WebNotificationItemEntity>(webNotificationItemEntity.Id, partKey).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new NotificationServiceException($"The deletion of entity with identifier '{entityId}' failed with status code response of '{response.StatusCode}'.");
            }

            isSuccess = true;
            this.logger.LogInformation($"Finished {nameof(this.DeleteAsync)} method of {nameof(WebNotificationItemEntityRepository)}.");
            return isSuccess;
        }

        /// <inheritdoc/>
        public async Task<WebNotificationItemEntity> ReadAsync(string entityId)
        {
            this.logger.LogInformation($"Started {nameof(this.ReadAsync)} method of {nameof(WebNotificationItemEntityRepository)}.");
            if (string.IsNullOrWhiteSpace(entityId))
            {
                throw new ArgumentException("The entity Id is not specified.", nameof(entityId));
            }

            WebNotificationItemEntity entity = await this.LoadEntity(entityId).ConfigureAwait(false);
            this.logger.LogInformation($"Finished {nameof(this.ReadAsync)} method of {nameof(WebNotificationItemEntityRepository)}.");
            return entity;
        }

        /// <inheritdoc/>
        public async Task<EntityCollection<WebNotificationItemEntity>> ReadAsync(Expression<Func<WebNotificationItemEntity, bool>> filterExpression, Expression<Func<WebNotificationItemEntity, NotificationPriority>> orderExpression, string nextPageId, int numOfEntities = 10)
        {
            this.logger.LogInformation($"Started {nameof(this.ReadAsync)} method of {nameof(WebNotificationItemEntityRepository)}.");
            IQueryable<WebNotificationItemEntity> query = this.BuildQuery(filterExpression, orderExpression, nextPageId, numOfEntities);
            EntityCollection<WebNotificationItemEntity> entities = await this.ExecuteQeury(query).ConfigureAwait(false);
            this.logger.LogInformation($"Finished {nameof(this.ReadAsync)} method of {nameof(WebNotificationItemEntityRepository)}.");
            return entities;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WebNotificationItemEntity>> UpsertAsync(IEnumerable<WebNotificationItemEntity> entities)
        {
            IEnumerable<WebNotificationItemEntity> webNotificationItemEntities = null;
            this.logger.LogInformation($"Started {nameof(this.UpsertAsync)} method of {nameof(WebNotificationItemEntityRepository)}.");
            if (entities is null)
            {
                throw new ArgumentException("There is no entity to persist.", nameof(entities));
            }

            if (entities.Any())
            {
                webNotificationItemEntities = await this.UpsertInternalAsync(entities).ConfigureAwait(false);
            }

            this.logger.LogInformation($"Finished {nameof(this.UpsertAsync)} method of {nameof(WebNotificationItemEntityRepository)}.");
            return webNotificationItemEntities;
        }

        private async Task<EntityCollection<WebNotificationItemEntity>> ExecuteQeury(IQueryable<WebNotificationItemEntity> query)
        {
            EntityCollection<WebNotificationItemEntity> entities = new EntityCollection<WebNotificationItemEntity>();
            FeedIterator<WebNotificationItemEntity> itemIterator = this.cosmosLinqQuery.GetFeedIterator(query);
            FeedResponse<WebNotificationItemEntity> feedResponse = await itemIterator.ReadNextAsync().ConfigureAwait(false);
            entities.Items = feedResponse.Resource;
            entities.NextPageId = feedResponse.ContinuationToken;
            return entities;
        }

        private async Task<WebNotificationItemEntity> LoadEntity(string entityId)
        {
            WebNotificationItemEntity entity = null;
            Expression<Func<WebNotificationItemEntity, bool>> filterExpression = notification => notification.NotificationId == entityId;
            WebNotificationItemEntity webNotificationItemEntity = (await this.ReadAsync(filterExpression, orderExpression: null, nextPageId: null).ConfigureAwait(false)).Items.FirstOrDefault();
            if (webNotificationItemEntity == null)
            {
                throw new NotificationServiceException($"The notification with notificationId '{entityId}' is not found.");
            }

            entity = webNotificationItemEntity;
            return entity;
        }

        private async Task<IEnumerable<WebNotificationItemEntity>> UpsertInternalAsync(IEnumerable<WebNotificationItemEntity> entities)
        {
            List<WebNotificationItemEntity> webNotificationItemEntities = new List<WebNotificationItemEntity>();
            List<Task<ItemResponse<WebNotificationItemEntity>>> tasks = new List<Task<ItemResponse<WebNotificationItemEntity>>>();
            foreach (var entity in entities)
            {
                tasks.Add(this.cosmosContainer.UpsertItemAsync(entity));
            }

            IEnumerable<ItemResponse<WebNotificationItemEntity>> itemResponses = await Task.WhenAll(tasks).ConfigureAwait(false);
            webNotificationItemEntities.AddRange(itemResponses.Where(ir => ir?.StatusCode == HttpStatusCode.OK || ir?.StatusCode == HttpStatusCode.Created).Select(ir => ir?.Resource));
            return webNotificationItemEntities;
        }

        private IQueryable<WebNotificationItemEntity> BuildQuery(Expression<Func<WebNotificationItemEntity, bool>> filterExpression, Expression<Func<WebNotificationItemEntity, NotificationPriority>> orderExpression, string nextPageId, int numOfEntities)
        {
            IQueryable<WebNotificationItemEntity> query = null;
            QueryRequestOptions queryRequestOptions = new QueryRequestOptions
            {
                MaxItemCount = numOfEntities,
            };

            if (!string.IsNullOrWhiteSpace(nextPageId))
            {
                query = this.cosmosContainer.GetItemLinqQueryable<WebNotificationItemEntity>(allowSynchronousQueryExecution: false, nextPageId, queryRequestOptions);
            }
            else
            {
                query = this.cosmosContainer.GetItemLinqQueryable<WebNotificationItemEntity>(allowSynchronousQueryExecution: false, null, queryRequestOptions);
            }

            if (filterExpression != null)
            {
                query = query.Where(filterExpression);
            }

            if (orderExpression != null)
            {
                query = query.OrderBy(orderExpression).ThenByDescending(notification => notification.PublishOnUTCDate);
            }
            else
            {
                query = query.OrderByDescending(notification => notification.PublishOnUTCDate);
            }

            return query;
        }
    }
}
