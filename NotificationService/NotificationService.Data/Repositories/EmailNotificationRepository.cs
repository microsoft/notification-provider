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
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Common.Utility;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Extensions;
    using NotificationService.Contracts.Models.Request;

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
        private readonly Container emailHistoryContainer;

        /// <summary>
        /// Instance of <see cref="Container"/>.
        /// </summary>
        private readonly Container meetingHistoryContainer;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of <see cref="ICosmosLinqQuery"/>.
        /// </summary>
        private readonly ICosmosLinqQuery cosmosLinqQuery;

        /// <summary>
        /// Instance of <see cref="IMailAttachmentRepository"/>.
        /// </summary>
        private readonly IMailAttachmentRepository mailAttachmentRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailNotificationRepository"/> class.
        /// </summary>
        /// <param name="cosmosDBSetting">Cosmos DB Configuration.</param>
        /// <param name="cosmosDBQueryClient">CosmosDB Query Client.</param>
        /// <param name="logger">Instance of Logger.</param>
        /// <param name="cosmosLinqQuery">Instance of Cosmos Linq query.</param>
        /// <param name="mailAttachmentRepository">Instance of the Mail Attachment repository.</param>
        public EmailNotificationRepository(IOptions<CosmosDBSetting> cosmosDBSetting, ICosmosDBQueryClient cosmosDBQueryClient, ILogger logger, ICosmosLinqQuery cosmosLinqQuery, IMailAttachmentRepository mailAttachmentRepository)
        {
            this.cosmosDBSetting = cosmosDBSetting?.Value ?? throw new System.ArgumentNullException(nameof(cosmosDBSetting));
            this.cosmosDBQueryClient = cosmosDBQueryClient ?? throw new System.ArgumentNullException(nameof(cosmosDBQueryClient));
            this.emailHistoryContainer = this.cosmosDBQueryClient.GetCosmosContainer(this.cosmosDBSetting.Database, this.cosmosDBSetting.EmailHistoryContainer);
            this.meetingHistoryContainer = this.cosmosDBQueryClient.GetCosmosContainer(this.cosmosDBSetting.Database, this.cosmosDBSetting.MeetingHistoryContainer);
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.cosmosLinqQuery = cosmosLinqQuery;
            this.mailAttachmentRepository = mailAttachmentRepository;
        }

        /// <inheritdoc/>
        public async Task CreateEmailNotificationItemEntities(IList<EmailNotificationItemEntity> emailNotificationItemEntities, string applicationName = null)
        {
            if (emailNotificationItemEntities is null)
            {
                throw new System.ArgumentNullException(nameof(emailNotificationItemEntities));
            }

            this.logger.TraceInformation($"Started {nameof(this.CreateEmailNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");

            IList<EmailNotificationItemEntity> updatedEmailNotificationItemEntities = emailNotificationItemEntities;
            if (applicationName != null)
            {
                updatedEmailNotificationItemEntities = await this.mailAttachmentRepository.UploadEmail(emailNotificationItemEntities, NotificationType.Mail.ToString(), applicationName).ConfigureAwait(false);
            }

            List<Task> createTasks = new List<Task>();
            foreach (var item in updatedEmailNotificationItemEntities)
            {
                createTasks.Add(this.emailHistoryContainer.CreateItemAsync(item.ConvertToEmailNotificationItemCosmosDbEntity()));
            }

            Task.WaitAll(createTasks.ToArray());
            this.logger.TraceInformation($"Finished {nameof(this.CreateEmailNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");

            return;
        }

        /// <inheritdoc/>
        public Task UpdateEmailNotificationItemEntities(IList<EmailNotificationItemEntity> emailNotificationItemEntities)
        {
            if (emailNotificationItemEntities is null)
            {
                throw new System.ArgumentNullException(nameof(emailNotificationItemEntities));
            }

            this.logger.TraceInformation($"Started {nameof(this.UpdateEmailNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");
            List<Task> updateTasks = new List<Task>();
            foreach (var item in emailNotificationItemEntities)
            {
                updateTasks.Add(this.emailHistoryContainer.UpsertItemAsync(item.ConvertToEmailNotificationItemCosmosDbEntity()));
            }

            Task.WaitAll(updateTasks.ToArray());
            this.logger.TraceInformation($"Finished {nameof(this.UpdateEmailNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<IList<EmailNotificationItemEntity>> GetEmailNotificationItemEntities(IList<string> notificationIds, string applicationName = null)
        {
            if (notificationIds is null)
            {
                throw new System.ArgumentNullException(nameof(notificationIds));
            }

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");
            List<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>();
            var query = this.emailHistoryContainer.GetItemLinqQueryable<EmailNotificationItemCosmosDbEntity>()
                .Where(nie => notificationIds.Contains(nie.NotificationId));

            var itemIterator = this.cosmosLinqQuery.GetFeedIterator(query);

            while (itemIterator.HasMoreResults)
            {
                foreach (var item in await itemIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    emailNotificationItemEntities.Add(item.ConvertToEmailNotificationItemEntity());
                }
            }

            IList<EmailNotificationItemEntity> updatedNotificationEntities = emailNotificationItemEntities;
            if (applicationName != null)
            {
                updatedNotificationEntities = await this.mailAttachmentRepository.DownloadEmail(emailNotificationItemEntities, applicationName).ConfigureAwait(false);
            }

            this.logger.TraceInformation($"Finished {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");
            return updatedNotificationEntities;
        }

        /// <inheritdoc/>
        public async Task<EmailNotificationItemEntity> GetEmailNotificationItemEntity(string notificationId, string applicationName = null)
        {
            if (notificationId is null)
            {
                throw new System.ArgumentNullException(nameof(notificationId));
            }

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(EmailNotificationRepository)}.");
            List<EmailNotificationItemEntity> emailNotificationItemEntities = new List<EmailNotificationItemEntity>();
            var query = this.emailHistoryContainer.GetItemLinqQueryable<EmailNotificationItemCosmosDbEntity>()
                .Where(nie => notificationId == nie.NotificationId);

            var itemIterator = this.cosmosLinqQuery.GetFeedIterator(query);

            while (itemIterator.HasMoreResults)
            {
                foreach (var item in await itemIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    emailNotificationItemEntities.Add(item.ConvertToEmailNotificationItemEntity());
                }
            }

            IList<EmailNotificationItemEntity> updatedNotificationEntities = emailNotificationItemEntities;
            if (applicationName != null)
            {
                updatedNotificationEntities = await this.mailAttachmentRepository.DownloadEmail(emailNotificationItemEntities, applicationName).ConfigureAwait(false);
            }

            if (updatedNotificationEntities.Count == 1)
            {
                this.logger.TraceInformation($"Finished {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(EmailNotificationRepository)}.");
                return updatedNotificationEntities.FirstOrDefault();
            }
            else if (updatedNotificationEntities.Count > 1)
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
            if (notificationReportRequest == null)
            {
                throw new System.ArgumentNullException(nameof(notificationReportRequest));
            }

            Expression<Func<EmailNotificationItemCosmosDbEntity, bool>> filterExpression = this.GetMailFilterExpression(notificationReportRequest);
            Expression<Func<EmailNotificationItemCosmosDbEntity, EmailNotificationItemCosmosDbEntity>> projectionExpression = this.GetEmailProjectionExpression();
            Expression<Func<EmailNotificationItemCosmosDbEntity, DateTime>> orderExpression = this.GetEmailOrderExpression();

            int skip = notificationReportRequest.Skip;
            int take = notificationReportRequest.Take > 0 ? notificationReportRequest.Take : 100;
            if (filterExpression == null)
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentNullException($"Filter Expression Cannot be null");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            if (projectionExpression == null)
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentNullException($"Select Expression Cannot be null");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            if (orderExpression == null)
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentNullException($"Order Expression Cannot be null");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotifications)} method of {nameof(EmailNotificationRepository)}.");

            var filteredNotifications = new List<EmailNotificationItemEntity>();
            var query = this.emailHistoryContainer.GetItemLinqQueryable<EmailNotificationItemCosmosDbEntity>()
                            .Where(filterExpression)
                            .OrderByDescending(orderExpression)
                            .Select(projectionExpression)
                            .Skip(skip)
                            .Take(take);

            var itemIterator = this.cosmosLinqQuery.GetFeedIterator<EmailNotificationItemCosmosDbEntity>(query);

            while (itemIterator.HasMoreResults)
            {
                foreach (EmailNotificationItemCosmosDbEntity item in await itemIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    filteredNotifications.Add(item.ConvertToEmailNotificationItemEntity());
                }
            }

            Tuple<IList<EmailNotificationItemEntity>, TableContinuationToken> tuple = new Tuple<IList<EmailNotificationItemEntity>, TableContinuationToken>(filteredNotifications, null);
            return tuple;
        }

        /// <inheritdoc/>
        public async Task<IList<EmailNotificationItemEntity>> GetPendingOrFailedEmailNotificationsByDateRange(DateTimeRange dateRange, string applicationName, List<NotificationItemStatus> statusList, bool loadBody = false)
        {
            if (dateRange == null || dateRange.StartDate == null || dateRange.EndDate == null)
            {
                throw new ArgumentNullException(nameof(dateRange));
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.ResendDateRange] = JsonConvert.SerializeObject(dateRange);

            this.logger.TraceInformation($"Started {nameof(this.GetPendingOrFailedEmailNotificationsByDateRange)} method of {nameof(EmailNotificationRepository)}.", traceProps);

            Expression<Func<EmailNotificationItemCosmosDbEntity, bool>> filterExpression = notification => true;

            filterExpression = filterExpression.And(notification => notification.SendOnUtcDate >= dateRange.StartDate).And(notification => notification.SendOnUtcDate <= dateRange.EndDate);

            if (!string.IsNullOrEmpty(applicationName))
            {
                filterExpression = filterExpression.And(notification => notification.Application == applicationName);
            }

            if (statusList?.Count > 0)
            {
                List<string> stringList = statusList.ConvertAll(f => f.ToString());
                filterExpression = filterExpression.And(notification => stringList.Contains(notification.Status));
            }

            Expression<Func<EmailNotificationItemCosmosDbEntity, EmailNotificationItemCosmosDbEntity>> projectionExpression = this.GetEmailProjectionExpression();
            Expression<Func<EmailNotificationItemCosmosDbEntity, DateTime>> orderExpression = this.GetEmailOrderExpression();
            if (projectionExpression == null)
            {
                throw new ArgumentNullException($"Select Expression Cannot be null");
            }

            if (orderExpression == null)
            {
                throw new ArgumentNullException($"Order Expression Cannot be null");
            }

            var filteredNotifications = new List<EmailNotificationItemEntity>();
            var query = this.emailHistoryContainer.GetItemLinqQueryable<EmailNotificationItemCosmosDbEntity>()
                            .Where(filterExpression)
                            .OrderByDescending(orderExpression)
                            .Select(projectionExpression);

            var itemIterator = this.cosmosLinqQuery.GetFeedIterator<EmailNotificationItemCosmosDbEntity>(query);

            while (itemIterator.HasMoreResults)
            {
                foreach (EmailNotificationItemCosmosDbEntity item in await itemIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    filteredNotifications.Add(item.ConvertToEmailNotificationItemEntity());
                }
            }

            IList<EmailNotificationItemEntity> updatedNotificationEntities = filteredNotifications;
            if (!string.IsNullOrEmpty(applicationName) && loadBody)
            {
                updatedNotificationEntities = await this.mailAttachmentRepository.DownloadEmail(filteredNotifications, applicationName).ConfigureAwait(false);
            }

            this.logger.TraceInformation($"Finished {nameof(this.GetPendingOrFailedEmailNotificationsByDateRange)} method of {nameof(EmailNotificationRepository)}.", traceProps);
            return updatedNotificationEntities;
        }

        private Expression<Func<EmailNotificationItemCosmosDbEntity, bool>> GetMailFilterExpression(NotificationReportRequest notificationReportRequest)
        {

            Expression<Func<EmailNotificationItemCosmosDbEntity, bool>> filterExpression = notification => true;

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

            if (notificationReportRequest.NotificationPriorityFilter?.Count > 0)
            {
                List<string> priorityList = notificationReportRequest.NotificationPriorityFilter.ToList().ConvertAll(f => f.ToString());
                filterExpression = filterExpression.And(notification => priorityList.Contains(notification.Priority));
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
                List<string> statusList = notificationReportRequest.NotificationStatusFilter.ToList().ConvertAll(f => f.ToString());
                filterExpression = filterExpression.And(notification => statusList.Contains(notification.Status));
            }

            if (notificationReportRequest.TrackingIdsFilter?.Count > 0)
            {
                filterExpression = filterExpression.And(notification => notificationReportRequest.TrackingIdsFilter.Contains(notification.TrackingId));
            }

            return filterExpression;
        }

        private Expression<Func<EmailNotificationItemCosmosDbEntity, DateTime>> GetEmailOrderExpression()
        {
            Expression<Func<EmailNotificationItemCosmosDbEntity, DateTime>> orderBy = notification => notification.SendOnUtcDate;
            return orderBy;
        }

        private Expression<Func<EmailNotificationItemCosmosDbEntity, EmailNotificationItemCosmosDbEntity>> GetEmailProjectionExpression()
        {
            Expression<Func<EmailNotificationItemCosmosDbEntity, EmailNotificationItemCosmosDbEntity>> selectExpression = n => new EmailNotificationItemCosmosDbEntity
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
                TemplateId = n.TemplateId,
                TrackingId = n.TrackingId,
                PartitionKey = n.PartitionKey,
                RowKey = n.RowKey,
                Id = n.Id,
            };
            return selectExpression;
        }

        /// <inheritdoc/>
        public async Task<IList<MeetingNotificationItemEntity>> GetMeetingNotificationItemEntities(IList<string> notificationIds, string applicationName = null)
        {
            if (notificationIds is null)
            {
                throw new System.ArgumentNullException(nameof(notificationIds));
            }

            this.logger.TraceInformation($"Started {nameof(this.GetMeetingNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");
            List<MeetingNotificationItemEntity> meetingNotificationItemEntities = new List<MeetingNotificationItemEntity>();
            var query = this.meetingHistoryContainer.GetItemLinqQueryable<MeetingNotificationItemCosmosDbEntity>()
                .Where(nie => notificationIds.Contains(nie.NotificationId));

            var itemIterator = this.cosmosLinqQuery.GetFeedIterator(query);

            while (itemIterator.HasMoreResults)
            {
                foreach (var item in await itemIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    meetingNotificationItemEntities.Add(item.ConvertToMeetingNotificationItemEntity());
                }
            }

            IList<MeetingNotificationItemEntity> updatedNotificationEntities = meetingNotificationItemEntities;
            if (applicationName != null)
            {
                updatedNotificationEntities = await this.mailAttachmentRepository.DownloadMeetingInvite(meetingNotificationItemEntities, applicationName).ConfigureAwait(false);
            }

            this.logger.TraceInformation($"Finished {nameof(this.GetMeetingNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");
            return updatedNotificationEntities;
        }

        /// <inheritdoc/>
        public async Task<MeetingNotificationItemEntity> GetMeetingNotificationItemEntity(string notificationId, string applicationName = null)
        {
            if (notificationId is null)
            {
                throw new System.ArgumentNullException(nameof(notificationId));
            }

            this.logger.TraceInformation($"Started {nameof(this.GetMeetingNotificationItemEntity)} method of {nameof(EmailNotificationRepository)}.");
            List<MeetingNotificationItemEntity> meetingNotificationItemEntities = new List<MeetingNotificationItemEntity>();
            var query = this.meetingHistoryContainer.GetItemLinqQueryable<MeetingNotificationItemCosmosDbEntity>()
                .Where(nie => notificationId == nie.NotificationId);

            var itemIterator = this.cosmosLinqQuery.GetFeedIterator(query);

            while (itemIterator.HasMoreResults)
            {
                foreach (var item in await itemIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    meetingNotificationItemEntities.Add(item.ConvertToMeetingNotificationItemEntity());
                }
            }

            IList<MeetingNotificationItemEntity> updatedNotificationEntities = meetingNotificationItemEntities;
            if (applicationName != null)
            {
                updatedNotificationEntities = await this.mailAttachmentRepository.DownloadMeetingInvite(meetingNotificationItemEntities, applicationName).ConfigureAwait(false);
            }

            if (updatedNotificationEntities.Count == 1)
            {
                this.logger.TraceInformation($"Finished {nameof(this.GetMeetingNotificationItemEntity)} method of {nameof(EmailNotificationRepository)}.");
                return updatedNotificationEntities.FirstOrDefault();
            }
            else if (updatedNotificationEntities.Count > 1)
            {
                throw new ArgumentException("More than one entity found for the input notification id: ", notificationId);
            }
            else
            {
                throw new ArgumentException("No entity found for the input notification id: ", notificationId);
            }
        }

        /// <inheritdoc/>
        public async Task CreateMeetingNotificationItemEntities(IList<MeetingNotificationItemEntity> meetingNotificationItemEntities, string applicationName = null)
        {
            if (meetingNotificationItemEntities is null)
            {
                throw new System.ArgumentNullException(nameof(meetingNotificationItemEntities));
            }

            this.logger.TraceInformation($"Started {nameof(this.CreateMeetingNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");

            IList<MeetingNotificationItemEntity> updatedMeetingNotificationItemEntities = meetingNotificationItemEntities;
            if (applicationName != null)
            {
                updatedMeetingNotificationItemEntities = await this.mailAttachmentRepository.UploadMeetingInvite(meetingNotificationItemEntities, applicationName).ConfigureAwait(false);
            }

            List<Task> createTasks = new List<Task>();
            foreach (var item in updatedMeetingNotificationItemEntities)
            {
                createTasks.Add(this.meetingHistoryContainer.CreateItemAsync(item.ConvertToMeetingNotificationItemCosmosDbEntity()));
            }

            Task.WaitAll(createTasks.ToArray());

            this.logger.TraceInformation($"Finished {nameof(this.CreateMeetingNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");

            return;
        }

        /// <inheritdoc/>
        public Task UpdateMeetingNotificationItemEntities(IList<MeetingNotificationItemEntity> meetingNotificationItemEntities)
        {
            if (meetingNotificationItemEntities is null)
            {
                throw new System.ArgumentNullException(nameof(meetingNotificationItemEntities));
            }

            this.logger.TraceInformation($"Started {nameof(this.UpdateMeetingNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");
            List<Task> updateTasks = new List<Task>();
            foreach (var item in meetingNotificationItemEntities)
            {
                updateTasks.Add(this.meetingHistoryContainer.UpsertItemAsync(item.ConvertToMeetingNotificationItemCosmosDbEntity()));
            }

            Task.WaitAll(updateTasks.ToArray());

            this.logger.TraceInformation($"Finished {nameof(this.UpdateMeetingNotificationItemEntities)} method of {nameof(EmailNotificationRepository)}.");

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<Tuple<IList<MeetingNotificationItemEntity>, Microsoft.Azure.Cosmos.Table.TableContinuationToken>> GetMeetingInviteNotifications(NotificationReportRequest meetingInviteReportRequest)
        {
            if (meetingInviteReportRequest is null)
            {
                throw new System.ArgumentNullException(nameof(meetingInviteReportRequest));
            }
            Expression<Func<MeetingNotificationItemCosmosDbEntity, bool>> filterExpression = this.GetMeetingFilterExpression(meetingInviteReportRequest);
            Expression<Func<MeetingNotificationItemCosmosDbEntity, MeetingNotificationItemCosmosDbEntity>> projectionExpression = this.GetMeetingProjectionExpression();
            Expression<Func<MeetingNotificationItemCosmosDbEntity, DateTime>> orderExpression = this.GetMeetingOrderExpression();
            int skip = meetingInviteReportRequest.Skip;
            int take = meetingInviteReportRequest.Take > 0 ? meetingInviteReportRequest.Take : 100;
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

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotifications)} method of {nameof(EmailNotificationRepository)}.");

            var filteredNotifications = new List<MeetingNotificationItemEntity>();
            var query = this.meetingHistoryContainer.GetItemLinqQueryable<MeetingNotificationItemCosmosDbEntity>()
                            .Where(filterExpression)
                            .OrderByDescending(orderExpression)
                            .Select(projectionExpression)
                            .Skip(skip)
                            .Take(take);

            var itemIterator = this.cosmosLinqQuery.GetFeedIterator<MeetingNotificationItemCosmosDbEntity>(query);

            while (itemIterator.HasMoreResults)
            {
                foreach (MeetingNotificationItemCosmosDbEntity item in await itemIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    filteredNotifications.Add(item.ConvertToMeetingNotificationItemEntity());
                }
            }

            Tuple<IList<MeetingNotificationItemEntity>, TableContinuationToken> tuple = new Tuple<IList<MeetingNotificationItemEntity>, TableContinuationToken>(filteredNotifications, null);
            return tuple;
        }

        /// <inheritdoc/>
        public async Task<IList<MeetingNotificationItemEntity>> GetPendingOrFailedMeetingNotificationsByDateRange(DateTimeRange dateRange, string applicationName, List<NotificationItemStatus> statusList, bool loadBody = false)
        {
            if (dateRange == null || dateRange.StartDate == null || dateRange.EndDate == null)
            {
                throw new ArgumentNullException(nameof(dateRange));
            }

            this.logger.TraceInformation($"Started {nameof(this.GetPendingOrFailedMeetingNotificationsByDateRange)} method of {nameof(EmailNotificationRepository)}");

            Expression<Func<MeetingNotificationItemCosmosDbEntity, bool>> filterExpression = notification => true;

            filterExpression = filterExpression.And(notification => notification.SendOnUtcDate >= dateRange.StartDate).And(notification => notification.SendOnUtcDate <= dateRange.EndDate);

            if (!string.IsNullOrEmpty(applicationName))
            {
                filterExpression = filterExpression.And(notification => notification.Application == applicationName);
            }

            if (statusList?.Count > 0)
            {
                List<string> stringList = statusList.ConvertAll(f => f.ToString());
                filterExpression = filterExpression.And(notification => stringList.Contains(notification.Status));
            }

            Expression<Func<MeetingNotificationItemCosmosDbEntity, MeetingNotificationItemCosmosDbEntity>> projectionExpression = this.GetMeetingProjectionExpression();
            Expression<Func<MeetingNotificationItemCosmosDbEntity, DateTime>> orderExpression = this.GetMeetingOrderExpression();
            if (projectionExpression == null)
            {
                throw new ArgumentNullException($"Select Expression Cannot be null");
            }

            if (orderExpression == null)
            {
                throw new ArgumentNullException($"Order Expression Cannot be null");
            }

            var filteredNotifications = new List<MeetingNotificationItemEntity>();
            var query = this.meetingHistoryContainer.GetItemLinqQueryable<MeetingNotificationItemCosmosDbEntity>()
                            .Where(filterExpression)
                            .OrderByDescending(orderExpression)
                            .Select(projectionExpression);

            var itemIterator = this.cosmosLinqQuery.GetFeedIterator<MeetingNotificationItemCosmosDbEntity>(query);

            while (itemIterator.HasMoreResults)
            {
                foreach (MeetingNotificationItemCosmosDbEntity item in await itemIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    filteredNotifications.Add(item.ConvertToMeetingNotificationItemEntity());
                }
            }

            IList<MeetingNotificationItemEntity> updatedNotificationEntities = filteredNotifications;
            if (!string.IsNullOrEmpty(applicationName) && loadBody)
            {
                updatedNotificationEntities = await this.mailAttachmentRepository.DownloadMeetingInvite(filteredNotifications, applicationName).ConfigureAwait(false);
            }

            this.logger.TraceInformation($"Finished {nameof(this.GetPendingOrFailedMeetingNotificationsByDateRange)} method of {nameof(EmailNotificationRepository)}.");
            return updatedNotificationEntities;
        }

        private Expression<Func<MeetingNotificationItemCosmosDbEntity, MeetingNotificationItemCosmosDbEntity>> GetMeetingProjectionExpression()
        {
            Expression<Func<MeetingNotificationItemCosmosDbEntity, MeetingNotificationItemCosmosDbEntity>> selectExpression = n => new MeetingNotificationItemCosmosDbEntity
            {
                NotificationId = n.NotificationId,
                Application = n.Application,
                Status = n.Status,
                TryCount = n.TryCount,
                ErrorMessage = n.ErrorMessage,
                CreatedDateTime = n.CreatedDateTime,
                SendOnUtcDate = n.SendOnUtcDate,
                TrackingId = n.TrackingId,
                Priority = n.Priority,
                RequiredAttendees = n.RequiredAttendees,
                From = n.From,
                OptionalAttendees = n.OptionalAttendees,
                Subject = n.Subject,
                ReminderMinutesBeforeStart = n.ReminderMinutesBeforeStart,
                Location = n.Location,
                Start = n.Start,
                End = n.End,
                EndDate = n.EndDate,
                RecurrencePattern = n.RecurrencePattern,
                ICalUid = n.ICalUid,
                Interval = n.Interval,
                DaysOfWeek = n.DaysOfWeek,
                DayofMonth = n.DayofMonth,
                DayOfWeekByMonth = n.DayOfWeekByMonth,
                MonthOfYear = n.MonthOfYear,
                Ocurrences = n.Ocurrences,
                IsAllDayEvent = n.IsAllDayEvent,
                IsOnlineMeeting = n.IsOnlineMeeting,
                IsResponseRequested = n.IsResponseRequested,
                IsCancel = n.IsCancel,
                IsPrivate = n.IsPrivate,
                TemplateId = n.TemplateId,
                OccurrenceId = n.OccurrenceId,
                SequenceNumber = n.SequenceNumber,
                AttachmentReference = n.AttachmentReference,
                EmailAccountUsed = n.EmailAccountUsed,
                EventId = n.EventId,
                Action = n.Action,
                PartitionKey = n.PartitionKey,
                RowKey = n.RowKey,
                Id = n.Id,
            };
            return selectExpression;
        }

        private Expression<Func<MeetingNotificationItemCosmosDbEntity, DateTime>> GetMeetingOrderExpression()
        {
            Expression<Func<MeetingNotificationItemCosmosDbEntity, DateTime>> orderBy = notification => notification.SendOnUtcDate;
            return orderBy;
        }

        private Expression<Func<MeetingNotificationItemCosmosDbEntity, bool>> GetMeetingFilterExpression(NotificationReportRequest notificationReportRequest)
        {

            Expression<Func<MeetingNotificationItemCosmosDbEntity, bool>> filterExpression = notification => true;

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

            if (notificationReportRequest.NotificationPriorityFilter?.Count > 0)
            {
                List<string> priorityList = notificationReportRequest.NotificationPriorityFilter.ToList().ConvertAll(f => f.ToString());
                filterExpression = filterExpression.And(notification => priorityList.Contains(notification.Priority));
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
                List<string> statusList = notificationReportRequest.NotificationStatusFilter.ToList().ConvertAll(f => f.ToString());
                filterExpression = filterExpression.And(notification => statusList.Contains(notification.Status));
            }

            if (notificationReportRequest.TrackingIdsFilter?.Count > 0)
            {
                filterExpression = filterExpression.And(notification => notificationReportRequest.TrackingIdsFilter.Contains(notification.TrackingId));
            }

            return filterExpression;
        }
    }
}
