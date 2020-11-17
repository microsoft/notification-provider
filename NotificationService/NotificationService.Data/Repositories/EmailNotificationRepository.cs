// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NotificationService.Common;
    using NotificationService.Common.Utility;
    using NotificationService.Contracts;

    /// <summary>
    /// Repository for Email Notifications.
    /// </summary>
    public class EmailNotificationRepository : IEmailNotificationRepository
    {
        /// <summary>
        /// Instance of Cosmos DB Configuration.
        /// </summary>
        private readonly CosmosDBSetting cosmosDBSetting;

        /// <summary>
        /// Instance of Application Configuration.
        /// </summary>
        private readonly ICosmosDBQueryClient cosmosDBQueryClient;

        /// <summary>
        /// Instance of <see cref="Container"/>.
        /// </summary>
        private readonly Container cosmosContainer;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        private readonly ICosmosLinqQuery cosmosLinqQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailNotificationRepository"/> class.
        /// </summary>
        /// <param name="cosmosDBSetting">Cosmos DB Configuration.</param>
        /// <param name="cosmosDBQueryClient">CosmosDB Query Client.</param>
        /// <param name="logger">Instance of Logger.</param>
        /// <param name="cosmosLinqQuery">Instance of Cosmos Linq query.</param>
        public EmailNotificationRepository(IOptions<CosmosDBSetting> cosmosDBSetting, ICosmosDBQueryClient cosmosDBQueryClient, ILogger<EmailNotificationRepository> logger, ICosmosLinqQuery cosmosLinqQuery)
        {
            this.cosmosDBSetting = cosmosDBSetting?.Value ?? throw new System.ArgumentNullException(nameof(cosmosDBSetting));
            this.cosmosDBQueryClient = cosmosDBQueryClient ?? throw new System.ArgumentNullException(nameof(cosmosDBQueryClient));
            this.cosmosContainer = this.cosmosDBQueryClient.GetCosmosContainer(this.cosmosDBSetting.Database, this.cosmosDBSetting.Container);
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.cosmosLinqQuery = cosmosLinqQuery;
        }

        /// <inheritdoc/>
        public Task CreateEmailNotificationItemEntities(IList<EmailNotificationItemEntity> emailNotificationItemEntities)
        {
            if (emailNotificationItemEntities is null)
            {
                throw new System.ArgumentNullException(nameof(emailNotificationItemEntities));
            }

            this.logger.LogInformation($"Started {nameof(this.CreateEmailNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");
            List<Task> createTasks = new List<Task>();
            foreach (var item in emailNotificationItemEntities)
            {
                createTasks.Add(this.cosmosContainer.CreateItemAsync(item));
            }

            Task.WaitAll(createTasks.ToArray());
            this.logger.LogInformation($"Finished {nameof(this.CreateEmailNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task UpdateEmailNotificationItemEntities(IList<EmailNotificationItemEntity> emailNotificationItemEntities)
        {
            if (emailNotificationItemEntities is null)
            {
                throw new System.ArgumentNullException(nameof(emailNotificationItemEntities));
            }

            this.logger.LogInformation($"Started {nameof(this.UpdateEmailNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");
            List<Task> updateTasks = new List<Task>();
            foreach (var item in emailNotificationItemEntities)
            {
                updateTasks.Add(this.cosmosContainer.UpsertItemAsync(item));
            }

            Task.WaitAll(updateTasks.ToArray());
            this.logger.LogInformation($"Finished {nameof(this.UpdateEmailNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<IList<EmailNotificationItemEntity>> GetEmailNotificationItemEntities(IList<string> notificationIds)
        {
            if (notificationIds is null)
            {
                throw new System.ArgumentNullException(nameof(notificationIds));
            }

            this.logger.LogInformation($"Started {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");
            List<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>();
            var query = this.cosmosContainer.GetItemLinqQueryable<EmailNotificationItemEntity>()
                .Where(nie => notificationIds.Contains(nie.NotificationId));

            var itemIterator = this.cosmosLinqQuery.GetFeedIterator(query);

            while (itemIterator.HasMoreResults)
            {
                foreach (var item in await itemIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    emailNotificationItemEntities.Add(item);
                }
            }

            this.logger.LogInformation($"Finished {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");
            return emailNotificationItemEntities;
        }

        /// <inheritdoc/>
        public async Task<EmailNotificationItemEntity> GetEmailNotificationItemEntity(string notificationId)
        {
            if (notificationId is null)
            {
                throw new System.ArgumentNullException(nameof(notificationId));
            }

            this.logger.LogInformation($"Started {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(EmailNotificationRepository)}.");
            List<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>();
            var query = this.cosmosContainer.GetItemLinqQueryable<EmailNotificationItemEntity>()
                .Where(nie => notificationId == nie.NotificationId);

            var itemIterator = this.cosmosLinqQuery.GetFeedIterator(query);

            while (itemIterator.HasMoreResults)
            {
                foreach (var item in await itemIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    emailNotificationItemEntities.Add(item);
                }
            }

            if (emailNotificationItemEntities.Count == 1)
            {
                this.logger.LogInformation($"Finished {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(EmailNotificationRepository)}.");
                return emailNotificationItemEntities.FirstOrDefault();
            }
            else if (emailNotificationItemEntities.Count > 1)
            {
                throw new ArgumentException("More than one entity found for the input notification id: ", notificationId);
            }
            else
            {
                throw new ArgumentException("No entity found for the input notification id: ", notificationId);
            }
        }

        /// <inheritdoc/>
        public async Task<Tuple<IList<EmailNotificationItemEntity>, Microsoft.Azure.Cosmos.Table.TableContinuationToken>> GetEmailNotifications(NotificationReportRequest notificationReportRequest)
        {
            Expression<Func<EmailNotificationItemEntity, bool>> filterExpression = this.GetFilterExpression(notificationReportRequest);
            Expression<Func<EmailNotificationItemEntity, EmailNotificationItemEntity>> projectionExpression = this.GetProjectionExpression();
            Expression<Func<EmailNotificationItemEntity, DateTime>> orderExpression = this.GetOrderExpression();
            int skip = notificationReportRequest.Skip;
            int take = notificationReportRequest.Take > 0 ? notificationReportRequest.Take : 100;
            if (filterExpression == null)
            {
                throw new ArgumentNullException($"Filter Expression Cannot be null");
            }

            if (projectionExpression == null)
            {
                throw new ArgumentNullException($"Select Expression Cannot be null");
            }

            if (orderExpression == null)
            {
                throw new ArgumentNullException($"Order Expression Cannot be null");
            }

            this.logger.LogInformation($"Started {nameof(this.GetEmailNotifications)} method of {nameof(EmailNotificationRepository)}.");

            var filteredNotifications = new List<EmailNotificationItemEntity>();
            var query = this.cosmosContainer.GetItemLinqQueryable<EmailNotificationItemEntity>()
                            .Where(filterExpression)
                            .OrderByDescending(orderExpression)
                            .Select(projectionExpression)
                            .Skip(skip)
                            .Take(take);

            var itemIterator = this.cosmosLinqQuery.GetFeedIterator<EmailNotificationItemEntity>(query);

            while (itemIterator.HasMoreResults)
            {
                foreach (EmailNotificationItemEntity item in await itemIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    filteredNotifications.Add(item);
                }
            }

            Tuple<IList<EmailNotificationItemEntity>, TableContinuationToken> tuple = new Tuple<IList<EmailNotificationItemEntity>, TableContinuationToken>(filteredNotifications, null);
            return tuple;
        }

        private Expression<Func<EmailNotificationItemEntity, bool>> GetFilterExpression(NotificationReportRequest notificationReportRequest)
        {
            Expression<Func<EmailNotificationItemEntity, bool>> filterExpression = notification => true;

            if (DateTime.TryParse(notificationReportRequest.CreatedDateTimeStart, out DateTime createdDateTimeStart))
            {
                filterExpression = filterExpression.And(notification => notification.CreatedDateTime >= createdDateTimeStart);
            }

            if (DateTime.TryParse(notificationReportRequest.CreatedDateTimeEnd, out DateTime createdDateTimeEnd))
            {
                filterExpression = filterExpression.And(notification => notification.CreatedDateTime <= createdDateTimeEnd);
            }

            if (DateTime.TryParse(notificationReportRequest.SendOnUtcDateStart, out DateTime sentTimeStart))
            {
                filterExpression = filterExpression.And(notification => notification.SendOnUtcDate >= sentTimeStart);
            }

            if (DateTime.TryParse(notificationReportRequest.SendOnUtcDateEnd, out DateTime sentTimeEnd))
            {
                filterExpression = filterExpression.And(notification => notification.SendOnUtcDate <= sentTimeEnd);
            }

            if (DateTime.TryParse(notificationReportRequest.UpdatedDateTimeStart, out DateTime updatedTimeStart))
            {
                filterExpression = filterExpression.And(notification => notification.UpdatedDateTime >= updatedTimeStart);
            }

            if (DateTime.TryParse(notificationReportRequest.UpdatedDateTimeEnd, out DateTime updatedTimeEnd))
            {
                filterExpression = filterExpression.And(notification => notification.UpdatedDateTime <= updatedTimeEnd);
            }

            if (notificationReportRequest.NotificationTypeFilter?.Count > 0)
            {
                filterExpression = filterExpression.And(notification => notificationReportRequest.NotificationTypeFilter.Contains(notification.NotifyType));
            }

            if (notificationReportRequest.NotificationPriorityFilter?.Count > 0)
            {
                filterExpression = filterExpression.And(notification => notificationReportRequest.NotificationPriorityFilter.Contains(notification.Priority));
            }

            if (notificationReportRequest.ApplicationFilter?.Count > 0)
            {
                filterExpression = filterExpression.And(notification => notificationReportRequest.ApplicationFilter.Contains(notification.Application));
            }

            if (notificationReportRequest.AccountsUsedFilter?.Count > 0)
            {
                filterExpression = filterExpression.And(notification => notificationReportRequest.AccountsUsedFilter.Contains(notification.EmailAccountUsed));
            }

            if (notificationReportRequest.NotificationIdsFilter?.Count > 0)
            {
                filterExpression = filterExpression.And(notification => notificationReportRequest.NotificationIdsFilter.Contains(notification.NotificationId));
            }

            if (notificationReportRequest.NotificationStatusFilter?.Count > 0)
            {
                filterExpression = filterExpression.And(notification => notificationReportRequest.NotificationStatusFilter.Contains(notification.Status));
            }

            if (notificationReportRequest.MailSensitivityFilter?.Count > 0)
            {
                filterExpression = filterExpression.And(notification => notificationReportRequest.MailSensitivityFilter.Contains(notification.Sensitivity));
            }

            return filterExpression;
        }

        private Expression<Func<EmailNotificationItemEntity, DateTime>> GetOrderExpression()
        {
            Expression<Func<EmailNotificationItemEntity, DateTime>> orderBy = notification => notification.SendOnUtcDate;
            return orderBy;
        }

        private Expression<Func<EmailNotificationItemEntity, EmailNotificationItemEntity>> GetProjectionExpression()
        {
            Expression<Func<EmailNotificationItemEntity, EmailNotificationItemEntity>> selectExpression = n => new EmailNotificationItemEntity
            {
                NotificationId = n.NotificationId,
                Application = n.Application,
                EmailAccountUsed = n.EmailAccountUsed,
                Status = n.Status,
                Priority = n.Priority,
                Sensitivity = n.Sensitivity,
                CreatedDateTime = n.CreatedDateTime,
                SendOnUtcDate = n.SendOnUtcDate,
                ErrorMessage = n.ErrorMessage,
                TryCount = n.TryCount,
                To = n.To,
                From = n.From,
                CC = n.CC,
                BCC = n.BCC,
                ReplyTo = n.ReplyTo,
                Subject = n.Subject,
                TemplateData = n.TemplateData,
                TemplateName = n.TemplateName,
                TrackingId = n.TrackingId,
            };
            return selectExpression;
        }
    }
}
