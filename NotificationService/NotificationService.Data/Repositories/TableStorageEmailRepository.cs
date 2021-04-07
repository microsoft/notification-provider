// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models.Request;


    /// <summary>
    /// Repository for TableStorage.
    /// </summary>
    public class TableStorageEmailRepository : IEmailNotificationRepository
    {
        /// <summary>
        /// Instance of StorageAccountSetting Configuration.
        /// </summary>
        private readonly StorageAccountSetting storageAccountSetting;

        /// <summary>
        /// Instance of Application Configuration.
        /// </summary>
        private readonly ITableStorageClient cloudStorageClient;

        /// <summary>
        /// Instance of <see cref="emailHistoryTable"/>.
        /// </summary>
        private readonly CloudTable emailHistoryTable;

        /// <summary>
        /// Instance of <see cref="meetingHistoryTable"/>.
        /// </summary>
        private readonly CloudTable meetingHistoryTable;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of <see cref="IMailAttachmentRepository"/>.
        /// </summary>
        private readonly IMailAttachmentRepository mailAttachmentRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageEmailRepository"/> class.
        /// </summary>
        /// <param name="storageAccountSetting">primary key of storage account.</param>
        /// <param name="cloudStorageClient"> cloud storage client for table storage.</param>
        /// <param name="logger">logger.</param>
        /// <param name="mailAttachmentRepository">Instnce of the Mail Attachment Repository.</param>
        public TableStorageEmailRepository(IOptions<StorageAccountSetting> storageAccountSetting, ITableStorageClient cloudStorageClient, ILogger logger, IMailAttachmentRepository mailAttachmentRepository)
        {
            this.storageAccountSetting = storageAccountSetting?.Value ?? throw new System.ArgumentNullException(nameof(storageAccountSetting));
            this.cloudStorageClient = cloudStorageClient ?? throw new System.ArgumentNullException(nameof(cloudStorageClient));
            var emailHistoryTableName = storageAccountSetting?.Value?.EmailHistoryTableName;
            var meetingHistoryTableName = storageAccountSetting?.Value?.MeetingHistoryTableName;
            if (string.IsNullOrEmpty(emailHistoryTableName))
            {
                throw new ArgumentNullException(nameof(storageAccountSetting), "EmailHistoryTableName property from StorageAccountSetting can't be null/empty. Please provide the value in appsettings.json file.");
            }

            if (string.IsNullOrEmpty(meetingHistoryTableName))
            {
                throw new ArgumentNullException(nameof(storageAccountSetting), "MeetingHistoryTableName property from StorageAccountSetting can't be null/empty. Please provide the value in appsettings.json file");
            }

            this.emailHistoryTable = this.cloudStorageClient.GetCloudTable(emailHistoryTableName);
            this.meetingHistoryTable = this.cloudStorageClient.GetCloudTable(meetingHistoryTableName);
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.mailAttachmentRepository = mailAttachmentRepository;
        }

        /// <inheritdoc/>
        public async Task CreateEmailNotificationItemEntities(IList<EmailNotificationItemEntity> emailNotificationItemEntities, string applicationName = null)
        {
            if (emailNotificationItemEntities is null || emailNotificationItemEntities.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(emailNotificationItemEntities));
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.EmailNotificationCount] = emailNotificationItemEntities?.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.CreateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            IList<EmailNotificationItemEntity> updatedEmailNotificationItemEntities = emailNotificationItemEntities;
            if (applicationName != null)
            {
                updatedEmailNotificationItemEntities = await this.mailAttachmentRepository.UploadEmail(emailNotificationItemEntities, NotificationType.Mail.ToString(), applicationName).ConfigureAwait(false);
            }

            var batchesToCreate = this.SplitList<EmailNotificationItemEntity>((List<EmailNotificationItemEntity>)updatedEmailNotificationItemEntities, ApplicationConstants.BatchSizeToStore).ToList();

            foreach (var batch in batchesToCreate)
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                foreach (var item in batch)
                {
                    batchOperation.Insert(this.ConvertToEmailNotificationItemTableEntity(item));
                }

                Task.WaitAll(this.emailHistoryTable.ExecuteBatchAsync(batchOperation));
            }

            this.logger.TraceInformation($"Finished {nameof(this.CreateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.", traceProps);

            return;
        }

        /// <inheritdoc/>
        public async Task<IList<EmailNotificationItemEntity>> GetEmailNotificationItemEntities(IList<string> notificationIds, string applicationName = null)
        {
            if (notificationIds is null || notificationIds.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(notificationIds));
            }

            string filterExpression = null;
            foreach (var item in notificationIds)
            {
                filterExpression = filterExpression == null ? TableQuery.GenerateFilterCondition("NotificationId", QueryComparisons.Equal, item) : filterExpression + " or " + TableQuery.GenerateFilterCondition("NotificationId", QueryComparisons.Equal, item);
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.NotificationIds] = JsonConvert.SerializeObject(notificationIds);

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            List<EmailNotificationItemTableEntity> emailNotificationItemEntities = new List<EmailNotificationItemTableEntity>();
            var linqQuery = new TableQuery<EmailNotificationItemTableEntity>().Where(filterExpression);
            emailNotificationItemEntities = this.emailHistoryTable.ExecuteQuery(linqQuery)?.Select(ent => ent).ToList();
            IList<EmailNotificationItemEntity> notificationEntities = emailNotificationItemEntities.Select(e => this.ConvertToEmailNotificationItemEntity(e)).ToList();
            IList<EmailNotificationItemEntity> updatedNotificationEntities = notificationEntities;
            if (applicationName != null)
            {
                updatedNotificationEntities = await this.mailAttachmentRepository.DownloadEmail(notificationEntities, applicationName).ConfigureAwait(false);
            }

            this.logger.TraceInformation($"Finished {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            return updatedNotificationEntities;
        }

        /// <inheritdoc/>
        public async Task<EmailNotificationItemEntity> GetEmailNotificationItemEntity(string notificationId, string applicationName = null)
        {
            if (notificationId is null)
            {
                throw new System.ArgumentNullException(nameof(notificationId));
            }

            var traceprops = new Dictionary<string, string>();
            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.NotificationIds] = notificationId;
            string filterExpression = TableQuery.GenerateFilterCondition("NotificationId", QueryComparisons.Equal, notificationId);

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(TableStorageEmailRepository)}.", traceprops);
            List<EmailNotificationItemTableEntity> emailNotificationItemEntities = new List<EmailNotificationItemTableEntity>();
            var linqQuery = new TableQuery<EmailNotificationItemTableEntity>().Where(filterExpression);
            emailNotificationItemEntities = this.emailHistoryTable.ExecuteQuery(linqQuery)?.Select(ent => ent).ToList();
            List<EmailNotificationItemEntity> notificationEntities = emailNotificationItemEntities.Select(e => this.ConvertToEmailNotificationItemEntity(e)).ToList();
            IList<EmailNotificationItemEntity> updatedNotificationEntities = notificationEntities;
            if (applicationName != null)
            {
                updatedNotificationEntities = await this.mailAttachmentRepository.DownloadEmail(notificationEntities, applicationName).ConfigureAwait(false);
            }

            this.logger.TraceInformation($"Finished {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(TableStorageEmailRepository)}.", traceprops);
            if (updatedNotificationEntities.Count == 1)
            {
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
        public Task<Tuple<IList<EmailNotificationItemEntity>, TableContinuationToken>> GetEmailNotifications(NotificationReportRequest notificationReportRequest)
        {
            if (notificationReportRequest == null)
            {
                throw new ArgumentNullException(nameof(notificationReportRequest));
            }

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotifications)} method of {nameof(TableStorageEmailRepository)}.");
            var entities = new List<EmailNotificationItemTableEntity>();
            var notificationEntities = new List<EmailNotificationItemEntity>();
            string filterDateExpression = this.GetDateFilterExpression(notificationReportRequest);
            string filterExpression = this.GetFilterExpression(notificationReportRequest);
            string finalFilter = filterDateExpression != null && filterDateExpression.Length > 0 ? filterDateExpression : filterExpression;
            if (filterDateExpression != null && filterDateExpression.Length > 0 && filterExpression != null && filterExpression.Length > 0)
            {
                finalFilter = TableQuery.CombineFilters(filterDateExpression, TableOperators.And, filterExpression);
            }

            var tableQuery = new TableQuery<EmailNotificationItemTableEntity>()
                     .Where(finalFilter)
                     .OrderByDesc(notificationReportRequest.SendOnUtcDateStart)
                     .Take(notificationReportRequest.Take == 0 ? 100 : notificationReportRequest.Take);
            var queryResult = this.emailHistoryTable.ExecuteQuerySegmented(tableQuery, notificationReportRequest.Token);
            entities.AddRange(queryResult.Results);
            notificationEntities = entities.Select(e => this.ConvertToEmailNotificationItemEntity(e)).ToList();
            var token = queryResult.ContinuationToken;
            Tuple<IList<EmailNotificationItemEntity>, TableContinuationToken> tuple = new Tuple<IList<EmailNotificationItemEntity>, TableContinuationToken>(notificationEntities, token);
            return Task.FromResult(tuple);
        }

        /// <inheritdoc/>
        public Task UpdateEmailNotificationItemEntities(IList<EmailNotificationItemEntity> emailNotificationItemEntities)
        {
            if (emailNotificationItemEntities is null || emailNotificationItemEntities.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(emailNotificationItemEntities));
            }

            this.logger.TraceInformation($"Started {nameof(this.UpdateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");
            TableBatchOperation batchOperation = new TableBatchOperation();
            foreach (var item in emailNotificationItemEntities)
            {
                batchOperation.Merge(this.ConvertToEmailNotificationItemTableEntity(item));
            }

            Task.WaitAll(this.emailHistoryTable.ExecuteBatchAsync(batchOperation));

            this.logger.TraceInformation($"Finished {nameof(this.UpdateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<IList<MeetingNotificationItemEntity>> GetMeetingNotificationItemEntities(IList<string> notificationIds, string applicationName)
        {
            if (notificationIds is null || notificationIds.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(notificationIds));
            }

            string filterExpression = null;
            foreach (var item in notificationIds)
            {
                filterExpression = filterExpression == null ? TableQuery.GenerateFilterCondition("NotificationId", QueryComparisons.Equal, item) : filterExpression + " or " + TableQuery.GenerateFilterCondition("NotificationId", QueryComparisons.Equal, item);
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.NotificationIds] = JsonConvert.SerializeObject(notificationIds);

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            List<MeetingNotificationItemTableEntity> meetingNotificationItemEntities = new List<MeetingNotificationItemTableEntity>();
            var linqQuery = new TableQuery<MeetingNotificationItemTableEntity>().Where(filterExpression);
            meetingNotificationItemEntities = this.meetingHistoryTable.ExecuteQuery(linqQuery).Select(ent => ent).ToList();

            IList<MeetingNotificationItemEntity> notificationEntities = meetingNotificationItemEntities.Select(e => this.ConvertToMeetingNotificationItemEntity(e)).ToList();
            IList<MeetingNotificationItemEntity> updatedNotificationEntities = await this.mailAttachmentRepository.DownloadMeetingInvite(notificationEntities, applicationName).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            return updatedNotificationEntities;
        }

        /// <inheritdoc/>
        public async Task<MeetingNotificationItemEntity> GetMeetingNotificationItemEntity(string notificationId, string applicationName)
        {
            if (notificationId is null)
            {
                throw new System.ArgumentNullException(nameof(notificationId));
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.NotificationIds] = notificationId;

            string filterExpression = TableQuery.GenerateFilterCondition("NotificationId", QueryComparisons.Equal, notificationId);
            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            List<MeetingNotificationItemTableEntity> meetingNotificationItemEntities = new List<MeetingNotificationItemTableEntity>();
            var linqQuery = new TableQuery<MeetingNotificationItemTableEntity>().Where(filterExpression);
            meetingNotificationItemEntities = this.meetingHistoryTable.ExecuteQuery(linqQuery).Select(ent => ent).ToList();
            List<MeetingNotificationItemEntity> notificationEntities = meetingNotificationItemEntities.Select(e => this.ConvertToMeetingNotificationItemEntity(e)).ToList();
            IList<MeetingNotificationItemEntity> updatedNotificationEntities = await this.mailAttachmentRepository.DownloadMeetingInvite(notificationEntities, applicationName).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            if (updatedNotificationEntities.Count == 1)
            {
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

        /// <summary>
        /// Creates the meeting notification item entities.
        /// </summary>
        /// <param name="meetingNotificationItemEntities">The meeting notification item entities.</param>
        /// <param name="applicationName"> application name as container name. </param>
        /// <returns>A <see cref="Task"/>.</returns>
        /// <exception cref="System.ArgumentNullException">meetingNotificationItemEntities.</exception>
        public async Task CreateMeetingNotificationItemEntities(IList<MeetingNotificationItemEntity> meetingNotificationItemEntities, string applicationName)
        {
            if (meetingNotificationItemEntities is null || meetingNotificationItemEntities.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(meetingNotificationItemEntities));
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.MeetingNotificationCount] = meetingNotificationItemEntities?.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.CreateMeetingNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            IList<MeetingNotificationItemEntity> updatedEmailNotificationItemEntities = await this.mailAttachmentRepository.UploadMeetingInvite(meetingNotificationItemEntities, NotificationType.Meet.ToString(), applicationName).ConfigureAwait(false);

            var batchesToCreate = this.SplitList<MeetingNotificationItemEntity>((List<MeetingNotificationItemEntity>)updatedEmailNotificationItemEntities, ApplicationConstants.BatchSizeToStore).ToList();

            foreach (var batch in batchesToCreate)
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                foreach (var item in batch)
                {
                    batchOperation.Insert(this.ConvertToMeetingNotificationItemTableEntity(item));
                }

                Task.WaitAll(this.meetingHistoryTable.ExecuteBatchAsync(batchOperation));
            }

            this.logger.TraceInformation($"Finished {nameof(this.CreateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            return;
        }

        /// <inheritdoc/>
        public Task UpdateMeetingNotificationItemEntities(IList<MeetingNotificationItemEntity> meetingNotificationItemEntities)
        {
            if (meetingNotificationItemEntities is null || meetingNotificationItemEntities.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(meetingNotificationItemEntities));
            }

            this.logger.TraceInformation($"Started {nameof(this.UpdateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");
            TableBatchOperation batchOperation = new TableBatchOperation();
            foreach (var item in meetingNotificationItemEntities)
            {
                batchOperation.Merge(this.ConvertToMeetingNotificationItemTableEntity(item));
            }

            Task.WaitAll(this.meetingHistoryTable.ExecuteBatchAsync(batchOperation));

            this.logger.TraceInformation($"Finished {nameof(this.UpdateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<Tuple<IList<MeetingNotificationItemEntity>, TableContinuationToken>> GetMeetingInviteNotifications(NotificationReportRequest meetingInviteReportRequest)
        {
            if (meetingInviteReportRequest == null)
            {
                throw new ArgumentNullException(nameof(meetingInviteReportRequest));
            }

            this.logger.TraceInformation($"Started {nameof(this.GetMeetingInviteNotifications)} method of {nameof(TableStorageEmailRepository)}.");
            var entities = new List<MeetingNotificationItemTableEntity>();
            var notificationEntities = new List<MeetingNotificationItemEntity>();
            string filterDateExpression = this.GetDateFilterExpression(meetingInviteReportRequest);
            string filterExpression = this.GetFilterExpression(meetingInviteReportRequest);
            string finalFilter = filterDateExpression != null && filterDateExpression.Length > 0 ? filterDateExpression : filterExpression;
            if (filterDateExpression != null && filterDateExpression.Length > 0 && filterExpression != null && filterExpression.Length > 0)
            {
                finalFilter = TableQuery.CombineFilters(filterDateExpression, TableOperators.And, filterExpression);
            }

            var tableQuery = new TableQuery<MeetingNotificationItemTableEntity>()
                     .Where(finalFilter)
                     .OrderByDesc(meetingInviteReportRequest.SendOnUtcDateStart)
                     .Take(meetingInviteReportRequest.Take == 0 ? 100 : meetingInviteReportRequest.Take);
            var queryResult = this.meetingHistoryTable.ExecuteQuerySegmented(tableQuery, meetingInviteReportRequest.Token);
            entities.AddRange(queryResult.Results);
            notificationEntities = entities.Select(e => this.ConvertToMeetingNotificationItemEntity(e)).ToList();
            var token = queryResult.ContinuationToken;
            Tuple<IList<MeetingNotificationItemEntity>, TableContinuationToken> tuple = new Tuple<IList<MeetingNotificationItemEntity>, TableContinuationToken>(notificationEntities, token);
            return Task.FromResult(tuple);
        }

        /// <inheritdoc/>
        public async Task<IList<EmailNotificationItemEntity>> GetPendingOrFailedEmailNotificationsByDateRange(DateTimeRange dateRange, string applicationName, List<NotificationItemStatus> statusList, bool loadBody = false)
        {
            if (dateRange == null || dateRange.StartDate == null || dateRange.EndDate == null)
            {
                throw new ArgumentNullException(nameof(dateRange));
            }

            string filterExpression = TableQuery.GenerateFilterConditionForDate("SendOnUtcDate", QueryComparisons.GreaterThanOrEqual, dateRange.StartDate)
                    + " and "
                    + TableQuery.GenerateFilterConditionForDate("SendOnUtcDate", QueryComparisons.LessThan, dateRange.EndDate);

            if (!string.IsNullOrEmpty(applicationName))
            {
                filterExpression = filterExpression
                    + " and "
                    + TableQuery.GenerateFilterCondition("Application", QueryComparisons.Equal, applicationName);
            }

            if (statusList != null && statusList.Count > 0)
            {
                string statusFilterExpression = null;
                foreach (var status in statusList)
                {
                    string filter = TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal, status.ToString());
                    statusFilterExpression = statusFilterExpression == null ? filter : " or " + filter;
                }

                filterExpression = TableQuery.CombineFilters(filterExpression, TableOperators.And, statusFilterExpression);
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.ResendDateRange] = JsonConvert.SerializeObject(dateRange);

            this.logger.TraceInformation($"Started {nameof(this.GetPendingOrFailedEmailNotificationsByDateRange)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            List<EmailNotificationItemTableEntity> emailNotificationItemEntities = new List<EmailNotificationItemTableEntity>();
            var linqQuery = new TableQuery<EmailNotificationItemTableEntity>().Where(filterExpression);
            emailNotificationItemEntities = this.emailHistoryTable.ExecuteQuery(linqQuery)?.Select(ent => ent).ToList();
            if (emailNotificationItemEntities == null || emailNotificationItemEntities.Count == 0)
            {
                return null;
            }

            IList<EmailNotificationItemEntity> notificationEntities = emailNotificationItemEntities.Select(e => this.ConvertToEmailNotificationItemEntity(e)).ToList();
            IList<EmailNotificationItemEntity> updatedNotificationEntities = notificationEntities;
            if (!string.IsNullOrEmpty(applicationName) && loadBody)
            {
                updatedNotificationEntities = await this.mailAttachmentRepository.DownloadEmail(notificationEntities, applicationName).ConfigureAwait(false);
            }

            this.logger.TraceInformation($"Finished {nameof(this.GetPendingOrFailedEmailNotificationsByDateRange)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            return updatedNotificationEntities;
        }

        private string GetFilterExpression(NotificationReportRequest notificationReportRequest)
        {
            var filterSet = new HashSet<string>();
            string filterExpression = null;
            string applicationFilter = null;
            string accountFilter = null;
            string notificationFilter = null;
            string statusFilter = null;
            string trackingIdFilter = null;

            if (notificationReportRequest.ApplicationFilter?.Count > 0)
            {
                foreach (var item in notificationReportRequest.ApplicationFilter)
                {
                    applicationFilter = applicationFilter == null ? TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, item) : applicationFilter + " or " + TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, item);
                }

                _ = filterSet.Add("(" + applicationFilter + ")");
            }

            if (notificationReportRequest.AccountsUsedFilter?.Count > 0)
            {
                foreach (var item in notificationReportRequest.AccountsUsedFilter)
                {
                    accountFilter = accountFilter == null ? TableQuery.GenerateFilterCondition("EmailAccountUsed", QueryComparisons.Equal, item) : accountFilter + " or " + TableQuery.GenerateFilterCondition("EmailAccountUsed", QueryComparisons.Equal, item);
                }

                _ = filterSet.Add("(" + accountFilter + ")");
            }

            if (notificationReportRequest.NotificationIdsFilter?.Count > 0)
            {
                foreach (var item in notificationReportRequest.NotificationIdsFilter)
                {
                    notificationFilter = notificationFilter == null ? TableQuery.GenerateFilterCondition("NotificationId", QueryComparisons.Equal, item) : notificationFilter + " or " + TableQuery.GenerateFilterCondition("NotificationId", QueryComparisons.Equal, item);
                }

                _ = filterSet.Add("(" + notificationFilter + ")");
            }

            if (notificationReportRequest.TrackingIdsFilter?.Count > 0)
            {
                foreach (var item in notificationReportRequest.TrackingIdsFilter)
                {
                    trackingIdFilter = trackingIdFilter == null ? TableQuery.GenerateFilterCondition("TrackingId", QueryComparisons.Equal, item) : trackingIdFilter + " or " + TableQuery.GenerateFilterCondition("TrackingId", QueryComparisons.Equal, item);
                }

                _ = filterSet.Add("(" + trackingIdFilter + ")");
            }

            if (notificationReportRequest.NotificationStatusFilter?.Count > 0)
            {
                foreach (int item in notificationReportRequest.NotificationStatusFilter)
                {
                    string status = GetStatus(item);
                    statusFilter = statusFilter == null ? TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal, status) : statusFilter + " or " + TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal, status);
                }

                _ = filterSet.Add("(" + statusFilter + ")");
            }

            filterExpression = PrepareFilterExp(filterSet);
            return filterExpression;

            static string PrepareFilterExp(HashSet<string> filterSet)
            {
                string filterExp = String.Join(" and ", filterSet.ToArray());
                return filterExp;
            }
        }

        /// <summary>
        /// Get notification status string.
        /// </summary>
        /// <param name="status">notification status int format.</param>
        /// <returns>returns notification status string. </returns>
        private static string GetStatus(int status)
        {
            string statusStr = "Queued";
            switch (status)
            {
                case 0:
                    statusStr = "Queued";
                    break;
                case 1:
                    statusStr = "Processing";
                    break;
                case 2:
                    statusStr = "Retrying";
                    break;
                case 3:
                    statusStr = "Failed";
                    break;
                case 4:
                    statusStr = "Sent";
                    break;
                case 5:
                    statusStr = "FakeMail";
                    break;
            }

            return statusStr;
        }

        private EmailNotificationItemTableEntity ConvertToEmailNotificationItemTableEntity(EmailNotificationItemEntity emailNotificationItemEntity)
        {
            EmailNotificationItemTableEntity emailNotificationItemTableEntity = new EmailNotificationItemTableEntity();
            emailNotificationItemTableEntity.PartitionKey = emailNotificationItemEntity.Application;
            emailNotificationItemTableEntity.RowKey = emailNotificationItemEntity.NotificationId;
            emailNotificationItemTableEntity.Application = emailNotificationItemEntity.Application;
            emailNotificationItemTableEntity.BCC = emailNotificationItemEntity.BCC;
            emailNotificationItemTableEntity.CC = emailNotificationItemEntity.CC;
            emailNotificationItemTableEntity.EmailAccountUsed = emailNotificationItemEntity.EmailAccountUsed;
            emailNotificationItemTableEntity.ErrorMessage = emailNotificationItemEntity.ErrorMessage;
            emailNotificationItemTableEntity.Footer = emailNotificationItemEntity.Footer;
            emailNotificationItemTableEntity.From = emailNotificationItemEntity.From;
            emailNotificationItemTableEntity.Header = emailNotificationItemEntity.Header;
            emailNotificationItemTableEntity.NotificationId = emailNotificationItemEntity.NotificationId;
            emailNotificationItemTableEntity.Priority = emailNotificationItemEntity.Priority.ToString();
            emailNotificationItemTableEntity.ReplyTo = emailNotificationItemEntity.ReplyTo;
            emailNotificationItemTableEntity.Sensitivity = emailNotificationItemEntity.Sensitivity;
            emailNotificationItemTableEntity.Status = emailNotificationItemEntity.Status.ToString();
            emailNotificationItemTableEntity.Subject = emailNotificationItemEntity.Subject;
            emailNotificationItemTableEntity.TemplateId = emailNotificationItemEntity.TemplateId;
            emailNotificationItemTableEntity.Timestamp = emailNotificationItemEntity.Timestamp;
            emailNotificationItemTableEntity.To = emailNotificationItemEntity.To;
            emailNotificationItemTableEntity.TrackingId = emailNotificationItemEntity.TrackingId;
            emailNotificationItemTableEntity.TryCount = emailNotificationItemEntity.TryCount;
            emailNotificationItemTableEntity.ETag = emailNotificationItemEntity.ETag;
            emailNotificationItemTableEntity.SendOnUtcDate = emailNotificationItemEntity.SendOnUtcDate;
            return emailNotificationItemTableEntity;
        }

        private MeetingNotificationItemTableEntity ConvertToMeetingNotificationItemTableEntity(MeetingNotificationItemEntity meetingNotificationItemEntity)
        {
            MeetingNotificationItemTableEntity meetingNotificationItemTableEntity = new MeetingNotificationItemTableEntity();
            meetingNotificationItemTableEntity.PartitionKey = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.RowKey = meetingNotificationItemEntity.NotificationId;
            meetingNotificationItemTableEntity.Application = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.RequiredAttendees = meetingNotificationItemEntity.RequiredAttendees;
            meetingNotificationItemTableEntity.OptionalAttendees = meetingNotificationItemEntity.OptionalAttendees;
            meetingNotificationItemTableEntity.Application = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.ErrorMessage = meetingNotificationItemEntity.ErrorMessage;
            meetingNotificationItemTableEntity.DayofMonth = meetingNotificationItemEntity.DayofMonth ?? default;
            meetingNotificationItemTableEntity.DayOfWeekByMonth = meetingNotificationItemEntity.DayOfWeekByMonth;
            meetingNotificationItemTableEntity.DaysOfWeek = meetingNotificationItemEntity.DaysOfWeek;
            meetingNotificationItemTableEntity.EndDate = meetingNotificationItemEntity.EndDate;
            meetingNotificationItemTableEntity.IsAllDayEvent = meetingNotificationItemEntity.IsAllDayEvent;
            meetingNotificationItemTableEntity.From = meetingNotificationItemEntity.From;
            meetingNotificationItemTableEntity.IsCancel = meetingNotificationItemEntity.IsCancel;
            meetingNotificationItemTableEntity.IsOnlineMeeting = meetingNotificationItemEntity.IsOnlineMeeting;
            meetingNotificationItemTableEntity.NotificationId = meetingNotificationItemEntity.NotificationId;
            meetingNotificationItemTableEntity.Priority = meetingNotificationItemEntity.Priority.ToString();
            meetingNotificationItemTableEntity.IsPrivate = meetingNotificationItemEntity.IsPrivate;
            meetingNotificationItemTableEntity.IsResponseRequested = meetingNotificationItemEntity.IsResponseRequested;
            meetingNotificationItemTableEntity.Status = meetingNotificationItemEntity.Status.ToString();
            meetingNotificationItemTableEntity.Subject = meetingNotificationItemEntity.Subject;
            meetingNotificationItemTableEntity.Location = meetingNotificationItemEntity.Location;
            meetingNotificationItemTableEntity.Timestamp = meetingNotificationItemEntity.Timestamp;
            meetingNotificationItemTableEntity.MonthOfYear = meetingNotificationItemEntity.MonthOfYear;
            meetingNotificationItemTableEntity.TrackingId = meetingNotificationItemEntity.TrackingId;
            meetingNotificationItemTableEntity.TryCount = meetingNotificationItemEntity.TryCount;
            meetingNotificationItemTableEntity.ETag = meetingNotificationItemEntity.ETag;
            meetingNotificationItemTableEntity.OccurrenceId = meetingNotificationItemEntity.OccurrenceId;
            meetingNotificationItemTableEntity.Ocurrences = meetingNotificationItemEntity.Ocurrences ?? default;
            meetingNotificationItemTableEntity.RecurrencePattern = meetingNotificationItemEntity.RecurrencePattern.ToString();
            meetingNotificationItemTableEntity.ReminderMinutesBeforeStart = meetingNotificationItemEntity.ReminderMinutesBeforeStart;
            meetingNotificationItemTableEntity.TemplateId = meetingNotificationItemEntity.TemplateId;
            meetingNotificationItemTableEntity.End = meetingNotificationItemEntity.End;
            meetingNotificationItemTableEntity.Start = meetingNotificationItemEntity.Start;
            meetingNotificationItemTableEntity.SequenceNumber = meetingNotificationItemEntity.SequenceNumber ?? default;
            meetingNotificationItemTableEntity.SendOnUtcDate = meetingNotificationItemEntity.SendOnUtcDate;
            meetingNotificationItemTableEntity.TrackingId = meetingNotificationItemEntity.TrackingId;
            meetingNotificationItemTableEntity.TryCount = meetingNotificationItemEntity.TryCount;
            meetingNotificationItemTableEntity.Timestamp = meetingNotificationItemEntity.Timestamp;
            meetingNotificationItemTableEntity.RowKey = meetingNotificationItemEntity.NotificationId;
            meetingNotificationItemTableEntity.PartitionKey = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.Interval = meetingNotificationItemEntity.Interval;
            meetingNotificationItemTableEntity.ICalUid = meetingNotificationItemEntity.ICalUid;
            meetingNotificationItemTableEntity.AttachmentReference = meetingNotificationItemEntity.AttachmentReference;
            meetingNotificationItemTableEntity.EmailAccountUsed = meetingNotificationItemEntity.EmailAccountUsed;
            meetingNotificationItemTableEntity.EventId = meetingNotificationItemEntity.EventId;
            return meetingNotificationItemTableEntity;
        }

        private MeetingNotificationItemEntity ConvertToMeetingNotificationItemEntity(MeetingNotificationItemTableEntity meetingNotificationItemTableEntity)
        {
            MeetingNotificationItemEntity meetingNotificationItemEntity = new MeetingNotificationItemEntity();
            meetingNotificationItemEntity.Application = meetingNotificationItemTableEntity.Application;
            meetingNotificationItemEntity.NotificationId = meetingNotificationItemTableEntity.NotificationId;
            meetingNotificationItemEntity.RequiredAttendees = meetingNotificationItemTableEntity.RequiredAttendees;
            meetingNotificationItemEntity.OptionalAttendees = meetingNotificationItemTableEntity.OptionalAttendees;
            meetingNotificationItemEntity.Application = meetingNotificationItemTableEntity.Application;
            meetingNotificationItemEntity.ErrorMessage = meetingNotificationItemTableEntity.ErrorMessage;
            meetingNotificationItemEntity.DayofMonth = meetingNotificationItemTableEntity.DayofMonth;
            meetingNotificationItemEntity.DayOfWeekByMonth = meetingNotificationItemTableEntity.DayOfWeekByMonth;
            meetingNotificationItemEntity.DaysOfWeek = meetingNotificationItemTableEntity.DaysOfWeek;
            meetingNotificationItemEntity.IsAllDayEvent = meetingNotificationItemTableEntity.IsAllDayEvent;
            meetingNotificationItemEntity.From = meetingNotificationItemTableEntity.From;
            meetingNotificationItemEntity.IsCancel = meetingNotificationItemTableEntity.IsCancel;
            meetingNotificationItemEntity.IsOnlineMeeting = meetingNotificationItemTableEntity.IsOnlineMeeting;
            meetingNotificationItemEntity.NotificationId = meetingNotificationItemTableEntity.NotificationId;
            meetingNotificationItemEntity.Priority = meetingNotificationItemTableEntity.Priority == null ? NotificationPriority.Low : (NotificationPriority)Enum.Parse(typeof(NotificationPriority), meetingNotificationItemTableEntity.Priority);
            meetingNotificationItemEntity.IsPrivate = meetingNotificationItemTableEntity.IsPrivate;
            meetingNotificationItemEntity.IsResponseRequested = meetingNotificationItemTableEntity.IsResponseRequested;
            meetingNotificationItemEntity.Status = meetingNotificationItemTableEntity.Status == null ? NotificationItemStatus.Queued : (NotificationItemStatus)Enum.Parse(typeof(NotificationItemStatus), meetingNotificationItemTableEntity.Status);
            meetingNotificationItemEntity.Subject = meetingNotificationItemTableEntity.Subject;
            meetingNotificationItemEntity.Location = meetingNotificationItemTableEntity.Location;
            meetingNotificationItemEntity.Timestamp = meetingNotificationItemTableEntity.Timestamp;
            meetingNotificationItemEntity.MonthOfYear = meetingNotificationItemTableEntity.MonthOfYear;
            meetingNotificationItemEntity.TrackingId = meetingNotificationItemTableEntity.TrackingId;
            meetingNotificationItemEntity.TryCount = meetingNotificationItemTableEntity.TryCount;
            meetingNotificationItemEntity.ETag = meetingNotificationItemTableEntity.ETag;
            meetingNotificationItemEntity.OccurrenceId = meetingNotificationItemTableEntity.OccurrenceId;
            meetingNotificationItemEntity.Ocurrences = meetingNotificationItemTableEntity.Ocurrences;
            meetingNotificationItemEntity.RecurrencePattern = meetingNotificationItemTableEntity.RecurrencePattern == null ? MeetingRecurrencePattern.None : (MeetingRecurrencePattern)Enum.Parse(typeof(MeetingRecurrencePattern), meetingNotificationItemTableEntity.RecurrencePattern);
            meetingNotificationItemEntity.ReminderMinutesBeforeStart = meetingNotificationItemTableEntity.ReminderMinutesBeforeStart;
            meetingNotificationItemEntity.TemplateId = meetingNotificationItemTableEntity.TemplateId;
            meetingNotificationItemEntity.End = meetingNotificationItemTableEntity.End;
            meetingNotificationItemEntity.Start = meetingNotificationItemTableEntity.Start;
            meetingNotificationItemEntity.EndDate = meetingNotificationItemTableEntity.EndDate;
            meetingNotificationItemEntity.SequenceNumber = meetingNotificationItemTableEntity.SequenceNumber;
            meetingNotificationItemEntity.SendOnUtcDate = meetingNotificationItemTableEntity.SendOnUtcDate;
            meetingNotificationItemEntity.TrackingId = meetingNotificationItemTableEntity.TrackingId;
            meetingNotificationItemEntity.TryCount = meetingNotificationItemTableEntity.TryCount;
            meetingNotificationItemEntity.Timestamp = meetingNotificationItemTableEntity.Timestamp;
            meetingNotificationItemEntity.RowKey = meetingNotificationItemTableEntity.NotificationId;
            meetingNotificationItemEntity.PartitionKey = meetingNotificationItemTableEntity.Application;
            meetingNotificationItemEntity.Interval = meetingNotificationItemTableEntity.Interval;
            meetingNotificationItemEntity.ICalUid = meetingNotificationItemTableEntity.ICalUid;
            meetingNotificationItemEntity.AttachmentReference = meetingNotificationItemTableEntity.AttachmentReference;
            meetingNotificationItemEntity.EventId = meetingNotificationItemTableEntity.EventId;
            meetingNotificationItemEntity.EmailAccountUsed = meetingNotificationItemTableEntity.EmailAccountUsed;
            meetingNotificationItemEntity.Action = meetingNotificationItemTableEntity.Action;
            return meetingNotificationItemEntity;
        }

        private EmailNotificationItemEntity ConvertToEmailNotificationItemEntity(EmailNotificationItemTableEntity emailNotificationItemTableEntity)
        {
            EmailNotificationItemEntity emailNotificationItemEntity = new EmailNotificationItemEntity();
            NotificationPriority notificationPriority = Enum.TryParse<NotificationPriority>(emailNotificationItemTableEntity.Priority, out notificationPriority) ? notificationPriority : NotificationPriority.Low;
            NotificationItemStatus notificationItemStatus = Enum.TryParse<NotificationItemStatus>(emailNotificationItemTableEntity.Status, out notificationItemStatus) ? notificationItemStatus : NotificationItemStatus.Queued;
            emailNotificationItemEntity.Priority = notificationPriority;
            emailNotificationItemEntity.Status = notificationItemStatus;
            emailNotificationItemEntity.PartitionKey = emailNotificationItemTableEntity.Application;
            emailNotificationItemEntity.RowKey = emailNotificationItemTableEntity.NotificationId;
            emailNotificationItemEntity.Application = emailNotificationItemTableEntity.Application;
            emailNotificationItemEntity.BCC = emailNotificationItemTableEntity.BCC;
            emailNotificationItemEntity.CC = emailNotificationItemTableEntity.CC;
            emailNotificationItemEntity.EmailAccountUsed = emailNotificationItemTableEntity.EmailAccountUsed;
            emailNotificationItemEntity.ErrorMessage = emailNotificationItemTableEntity.ErrorMessage;
            emailNotificationItemEntity.Footer = emailNotificationItemTableEntity.Footer;
            emailNotificationItemEntity.From = emailNotificationItemTableEntity.From;
            emailNotificationItemEntity.Header = emailNotificationItemTableEntity.Header;
            emailNotificationItemEntity.NotificationId = emailNotificationItemTableEntity.NotificationId;
            emailNotificationItemEntity.ReplyTo = emailNotificationItemTableEntity.ReplyTo;
            emailNotificationItemEntity.Sensitivity = emailNotificationItemTableEntity.Sensitivity;
            emailNotificationItemEntity.Subject = emailNotificationItemTableEntity.Subject;
            emailNotificationItemEntity.TemplateId = emailNotificationItemTableEntity.TemplateId;
            //emailNotificationItemEntity.TemplateData = emailNotificationItemTableEntity.TemplateData;
            emailNotificationItemEntity.Timestamp = emailNotificationItemTableEntity.Timestamp;
            emailNotificationItemEntity.To = emailNotificationItemTableEntity.To;
            emailNotificationItemEntity.TrackingId = emailNotificationItemTableEntity.TrackingId;
            emailNotificationItemEntity.TryCount = emailNotificationItemTableEntity.TryCount;
            emailNotificationItemEntity.ETag = emailNotificationItemTableEntity.ETag;
            emailNotificationItemEntity.SendOnUtcDate = emailNotificationItemTableEntity.SendOnUtcDate;
            return emailNotificationItemEntity;
        }

        private string GetDateFilterExpression(NotificationReportRequest notificationReportRequest)
        {
            string filterExpression = null;
            if (DateTime.TryParse(notificationReportRequest.CreatedDateTimeStart, out DateTime createdDateTimeStart))
            {
                filterExpression = filterExpression == null ? TableQuery.GenerateFilterConditionForDate("CreatedDateTimeStart", QueryComparisons.GreaterThanOrEqual, createdDateTimeStart) : filterExpression + TableQuery.GenerateFilterConditionForDate("CreatedDateTimeStart", QueryComparisons.GreaterThanOrEqual, createdDateTimeStart);
            }

            if (DateTime.TryParse(notificationReportRequest.CreatedDateTimeEnd, out DateTime createdDateTimeEnd))
            {
                filterExpression = filterExpression == null ? TableQuery.GenerateFilterConditionForDate("CreatedDateTimeEnd", QueryComparisons.LessThanOrEqual, createdDateTimeEnd) : filterExpression + TableQuery.GenerateFilterConditionForDate("CreatedDateTimeEnd", QueryComparisons.LessThanOrEqual, createdDateTimeEnd);
            }

            if (DateTime.TryParse(notificationReportRequest.SendOnUtcDateStart, out DateTime sentTimeStart))
            {
                filterExpression = filterExpression == null ? TableQuery.GenerateFilterConditionForDate("SendOnUtcDate", QueryComparisons.GreaterThanOrEqual, sentTimeStart) : filterExpression + " and " + TableQuery.GenerateFilterConditionForDate("SendOnUtcDate", QueryComparisons.GreaterThanOrEqual, sentTimeStart);
            }

            if (DateTime.TryParse(notificationReportRequest.SendOnUtcDateEnd, out DateTime sentTimeEnd))
            {
                filterExpression = filterExpression == null ? TableQuery.GenerateFilterConditionForDate("SendOnUtcDate", QueryComparisons.LessThanOrEqual, sentTimeEnd) : filterExpression + " and " + TableQuery.GenerateFilterConditionForDate("SendOnUtcDate", QueryComparisons.LessThanOrEqual, sentTimeEnd);
            }

            if (DateTime.TryParse(notificationReportRequest.UpdatedDateTimeStart, out DateTime updatedTimeStart))
            {
                filterExpression = filterExpression == null ? TableQuery.GenerateFilterConditionForDate("UpdatedDateTimeStart", QueryComparisons.GreaterThanOrEqual, updatedTimeStart) : filterExpression + TableQuery.GenerateFilterConditionForDate("UpdatedDateTimeStart", QueryComparisons.GreaterThanOrEqual, updatedTimeStart);
            }

            if (DateTime.TryParse(notificationReportRequest.UpdatedDateTimeEnd, out DateTime updatedTimeEnd))
            {
                filterExpression = filterExpression == null ? TableQuery.GenerateFilterConditionForDate("UpdatedDateTimeEnd", QueryComparisons.LessThanOrEqual, updatedTimeEnd) : filterExpression + TableQuery.GenerateFilterConditionForDate("UpdatedDateTimeEnd", QueryComparisons.LessThanOrEqual, updatedTimeEnd);
            }

            return filterExpression;
        }

        /// <summary>
        /// Breaks the input list to multiple chunks each of size provided as input.
        /// </summary>
        /// <typeparam name="T">Type of object in the List.</typeparam>
        /// <param name="listItems">List of objects.</param>
        /// <param name="nSize">Chunk size.</param>
        /// <returns>An enumerable collection of chunks.</returns>
        private IEnumerable<IList<T>> SplitList<T>(List<T> listItems, int nSize = 4)
        {
            if (listItems is null)
            {
                throw new ArgumentNullException(nameof(listItems));
            }

            for (int i = 0; i < listItems.Count; i += nSize)
            {
                yield return listItems.GetRange(i, Math.Min(nSize, listItems.Count - i));
            }
        }
    }
}
