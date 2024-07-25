// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using Newtonsoft.Json;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Extensions;
    using NotificationService.Contracts.Models;
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

        private readonly string _emailHistoryTableName;

        private readonly string _meetingHistoryTableName;


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
            this._emailHistoryTableName = storageAccountSetting?.Value?.EmailHistoryTableName;
            this._meetingHistoryTableName = storageAccountSetting?.Value?.MeetingHistoryTableName;
            if (string.IsNullOrEmpty(this._emailHistoryTableName))
            {
                throw new ArgumentNullException(nameof(storageAccountSetting), "EmailHistoryTableName property from StorageAccountSetting can't be null/empty. Please provide the value in appsettings.json file.");
            }

            if (string.IsNullOrEmpty(this._meetingHistoryTableName))
            {
                throw new ArgumentNullException(nameof(storageAccountSetting), "MeetingHistoryTableName property from StorageAccountSetting can't be null/empty. Please provide the value in appsettings.json file");
            }
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
                IList<EmailNotificationItemTableEntity> lstEmailNotificationItemTable = new List<EmailNotificationItemTableEntity>();
                foreach (var item in batch)
                {
                    lstEmailNotificationItemTable.Add(item.ConvertToEmailNotificationItemTableEntity());
                }
                await cloudStorageClient.InsertRecordsAsync<EmailNotificationItemTableEntity>(this._emailHistoryTableName, lstEmailNotificationItemTable.ToList());
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

             var traceProps = new Dictionary<string, string>();
             traceProps[AIConstants.Application] = applicationName;
             traceProps[AIConstants.NotificationIds] = JsonConvert.SerializeObject(notificationIds);

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            List<EmailNotificationItemTableEntity> emailNotificationItemEntities = new List<EmailNotificationItemTableEntity>();
            foreach (var item in notificationIds)
            {
                var tableResult = await cloudStorageClient.GetRecordAsync<EmailNotificationItemTableEntity>(this._emailHistoryTableName, applicationName, item);
                emailNotificationItemEntities.Add((EmailNotificationItemTableEntity)tableResult);
            }

            IList<EmailNotificationItemEntity> notificationEntities = emailNotificationItemEntities.Select(e => e.ConvertToEmailNotificationItemEntity()).ToList();
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
            string filterExpression = $"NotificationId eq '{notificationId}'";

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(TableStorageEmailRepository)}.", traceprops);
          
            var emailNotificationItemEntities = await cloudStorageClient.GetRecordsAsync<EmailNotificationItemTableEntity>(this._emailHistoryTableName, filterExpression);
            emailNotificationItemEntities = emailNotificationItemEntities.Select(ent => ent).ToList();


            List<EmailNotificationItemEntity> notificationEntities = emailNotificationItemEntities.Select(e => e.ConvertToEmailNotificationItemEntity()).ToList();
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
        public Task<Tuple<IList<EmailNotificationItemEntity>, string>> GetEmailNotifications(NotificationReportRequest notificationReportRequest)
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
                finalFilter = CombineFilters(filterDateExpression, "and", filterExpression);
            }

            var mailTemplateEntities = cloudStorageClient.GetRecordsAsync<EmailNotificationItemTableEntity>(this._emailHistoryTableName, finalFilter).Result;

            notificationEntities = mailTemplateEntities.Select(e => e.ConvertToEmailNotificationItemEntity()).ToList();
            var token = notificationReportRequest.Token;
            Tuple<IList<EmailNotificationItemEntity>, string> tuple = new Tuple<IList<EmailNotificationItemEntity>, string>(notificationEntities, token);
            return Task.FromResult(tuple);
        }

        /// <inheritdoc/>
        public async Task UpdateEmailNotificationItemEntities(IList<EmailNotificationItemEntity> emailNotificationItemEntities)
        {
            if (emailNotificationItemEntities is null || emailNotificationItemEntities.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(emailNotificationItemEntities));
            }

            this.logger.TraceInformation($"Started {nameof(this.UpdateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");
            IList<EmailNotificationItemTableEntity> lstEmailNotificationItem = new List<EmailNotificationItemTableEntity>();
            foreach (var item in emailNotificationItemEntities)
            {
                lstEmailNotificationItem.Add(item.ConvertToEmailNotificationItemTableEntity());
            }
            await cloudStorageClient.AddsOrUpdatesAsync<EmailNotificationItemTableEntity>(this._emailHistoryTableName, lstEmailNotificationItem.ToList());

            this.logger.TraceInformation($"Finished {nameof(this.UpdateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");

            return;
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
                filterExpression = filterExpression == null ? $"NotificationId eq '{item}'" : filterExpression + " or " + $"NotificationId eq '{item}'";
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.NotificationIds] = JsonConvert.SerializeObject(notificationIds);

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            var meetingNotificationItemEntities = await cloudStorageClient.GetRecordsAsync<MeetingNotificationItemTableEntity>(this._meetingHistoryTableName, filterExpression);
            meetingNotificationItemEntities = meetingNotificationItemEntities.Select(ent => ent).ToList();

            IList<MeetingNotificationItemEntity> notificationEntities = meetingNotificationItemEntities.Select(e => e.ConvertToMeetingNotificationItemEntity()).ToList();
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
            string filterExpression = $"NotificationId eq '{notificationId}'";

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
          
            var meetingNotificationItemEntities = await cloudStorageClient.GetRecordsAsync<MeetingNotificationItemTableEntity>(this._meetingHistoryTableName, filterExpression);
            meetingNotificationItemEntities = meetingNotificationItemEntities.Select(ent => ent).ToList();

            List<MeetingNotificationItemEntity> notificationEntities = meetingNotificationItemEntities.Select(e => e.ConvertToMeetingNotificationItemEntity()).ToList();
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
            IList<MeetingNotificationItemEntity> updatedEmailNotificationItemEntities = await this.mailAttachmentRepository.UploadMeetingInvite(meetingNotificationItemEntities, applicationName).ConfigureAwait(false);

            var batchesToCreate = this.SplitList<MeetingNotificationItemEntity>((List<MeetingNotificationItemEntity>)updatedEmailNotificationItemEntities, ApplicationConstants.BatchSizeToStore).ToList();
            foreach (var batch in batchesToCreate)
            {
                IList<MeetingNotificationItemTableEntity> lstConvertedMeetingNotificationItem = new List<MeetingNotificationItemTableEntity>();
                foreach (var item in batch)
                {
                    lstConvertedMeetingNotificationItem.Add(item.ConvertToMeetingNotificationItemTableEntity());
                }
                await cloudStorageClient.InsertRecordsAsync<MeetingNotificationItemTableEntity>(this._meetingHistoryTableName, lstConvertedMeetingNotificationItem.ToList());
            }
            this.logger.TraceInformation($"Finished {nameof(this.CreateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            return;
        }

        /// <inheritdoc/>
        public async Task UpdateMeetingNotificationItemEntities(IList<MeetingNotificationItemEntity> meetingNotificationItemEntities)
        {
            if (meetingNotificationItemEntities is null || meetingNotificationItemEntities.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(meetingNotificationItemEntities));
            }

            this.logger.TraceInformation($"Started {nameof(this.UpdateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");
            IList<MeetingNotificationItemTableEntity> lstMeetingNotificationItem = new List<MeetingNotificationItemTableEntity>();
            foreach (var item in meetingNotificationItemEntities)
            {
                lstMeetingNotificationItem.Add(item.ConvertToMeetingNotificationItemTableEntity());
            }
            await cloudStorageClient.AddsOrUpdatesAsync<MeetingNotificationItemTableEntity>(this._meetingHistoryTableName, lstMeetingNotificationItem.ToList());

            this.logger.TraceInformation($"Finished {nameof(this.UpdateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");

            return;
        }

        /// <inheritdoc/>
        public Task<Tuple<IList<MeetingNotificationItemEntity>, string>> GetMeetingInviteNotifications(NotificationReportRequest meetingInviteReportRequest)
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
                finalFilter = CombineFilters(filterDateExpression, " and ", filterExpression);
            }

            var mailTemplateEntities = cloudStorageClient.GetRecordsAsync<MeetingNotificationItemTableEntity>(this._meetingHistoryTableName, finalFilter).Result;

            notificationEntities = mailTemplateEntities.Select(e => e.ConvertToMeetingNotificationItemEntity()).ToList();
            Tuple<IList<MeetingNotificationItemEntity>, string> tuple = new Tuple<IList<MeetingNotificationItemEntity>, string>(notificationEntities, meetingInviteReportRequest.Token);
            return Task.FromResult(tuple);
        }

        /// <inheritdoc/>
        public async Task<IList<EmailNotificationItemEntity>> GetPendingOrFailedEmailNotificationsByDateRange(DateTimeRange dateRange, string applicationName, List<NotificationItemStatus> statusList, bool loadBody = false)
        {
            if (dateRange == null || dateRange.StartDate == null || dateRange.EndDate == null)
            {
                throw new ArgumentNullException(nameof(dateRange));
            }

            string filterExpression = $"SendOnUtcDate ge {GenerateFilterForDate(dateRange.StartDate)} and SendOnUtcDate lt {GenerateFilterForDate(dateRange.EndDate)}";

            if (!string.IsNullOrEmpty(applicationName))
            {
                filterExpression = filterExpression
                    + " and "
                    + $"Application eq '{applicationName}'";
            }

            if (statusList != null && statusList.Count > 0)
            {
                string statusFilterExpression = null;
                foreach (var status in statusList)
                {
                    string filter = $"Status eq '{status.ToString()}'";
                    statusFilterExpression = statusFilterExpression == null ? filter : " or " + filter;
                }

                filterExpression = $"{filterExpression} and {statusFilterExpression}";
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.ResendDateRange] = JsonConvert.SerializeObject(dateRange);

            this.logger.TraceInformation($"Started {nameof(this.GetPendingOrFailedEmailNotificationsByDateRange)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            
            var emailNotificationItemEntities = await cloudStorageClient.GetRecordsAsync<EmailNotificationItemTableEntity>(this._emailHistoryTableName, filterExpression);


            if (emailNotificationItemEntities == null || emailNotificationItemEntities.Count == 0)
            {
                return null;
            }

            IList<EmailNotificationItemEntity> notificationEntities = emailNotificationItemEntities.Select(e => e.ConvertToEmailNotificationItemEntity()).ToList();
            IList<EmailNotificationItemEntity> updatedNotificationEntities = notificationEntities;
            if (!string.IsNullOrEmpty(applicationName) && loadBody)
            {
                updatedNotificationEntities = await this.mailAttachmentRepository.DownloadEmail(notificationEntities, applicationName).ConfigureAwait(false);
            }

            this.logger.TraceInformation($"Finished {nameof(this.GetPendingOrFailedEmailNotificationsByDateRange)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            return updatedNotificationEntities;
        }

        /// <inheritdoc/>
        public async Task<IList<MeetingNotificationItemEntity>> GetPendingOrFailedMeetingNotificationsByDateRange(DateTimeRange dateRange, string applicationName, List<NotificationItemStatus> statusList, bool loadBody = false)
        {
            if (dateRange == null || dateRange.StartDate == null || dateRange.EndDate == null)
            {
                throw new ArgumentNullException(nameof(dateRange));
            }
            string filterExpression = $"SendOnUtcDate ge {GenerateFilterForDate(dateRange.StartDate)} and SendOnUtcDate lt {GenerateFilterForDate(dateRange.EndDate)}";


            if (!string.IsNullOrEmpty(applicationName))
            {
                filterExpression = filterExpression
                    + " and "
                    + $"Application eq '{applicationName}'";
            }

            if (statusList != null && statusList.Count > 0)
            {
                string statusFilterExpression = null;
                foreach (var status in statusList)
                {
                    string filter = $"Status eq '{status.ToString()}'";
                    statusFilterExpression = statusFilterExpression == null ? filter : " or " + filter;
                }

                filterExpression = $"{filterExpression} and {statusFilterExpression}";
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.ResendDateRange] = JsonConvert.SerializeObject(dateRange);

            this.logger.TraceInformation($"Started {nameof(this.GetPendingOrFailedMeetingNotificationsByDateRange)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
          
            var meetingNotificationItemEntities = await cloudStorageClient.GetRecordsAsync<MeetingNotificationItemTableEntity>(this._meetingHistoryTableName, filterExpression);
            meetingNotificationItemEntities = meetingNotificationItemEntities?.Select(ent => ent).ToList();

            if (meetingNotificationItemEntities == null || meetingNotificationItemEntities.Count == 0)
            {
                return null;
            }

            IList<MeetingNotificationItemEntity> notificationEntities = meetingNotificationItemEntities.Select(e => e.ConvertToMeetingNotificationItemEntity()).ToList();
            IList<MeetingNotificationItemEntity> updatedNotificationEntities = notificationEntities;
            if (!string.IsNullOrEmpty(applicationName) && loadBody)
            {
                updatedNotificationEntities = await this.mailAttachmentRepository.DownloadMeetingInvite(notificationEntities, applicationName).ConfigureAwait(false);
            }

            this.logger.TraceInformation($"Finished {nameof(this.GetPendingOrFailedMeetingNotificationsByDateRange)} method of {nameof(TableStorageEmailRepository)}.", traceProps);
            return updatedNotificationEntities;
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
                    applicationFilter = applicationFilter == null ? $"PartitionKey eq '{item}'" : applicationFilter + " or " + $"PartitionKey eq '{item}'";
                }

                _ = filterSet.Add("(" + applicationFilter + ")");
            }

            if (notificationReportRequest.AccountsUsedFilter?.Count > 0)
            {
                foreach (var item in notificationReportRequest.AccountsUsedFilter)
                {
                    accountFilter = accountFilter == null ? $"EmailAccountUsed eq '{item}'" : accountFilter + " or " + $"EmailAccountUsed eq '{item}'";
                }

                _ = filterSet.Add("(" + accountFilter + ")");
            }

            if (notificationReportRequest.NotificationIdsFilter?.Count > 0)
            {
                foreach (var item in notificationReportRequest.NotificationIdsFilter)
                {
                    notificationFilter = notificationFilter == null ? $"NotificationId eq '{item}'" : notificationFilter + " or " + $"NotificationId eq '{item}'";
                }

                _ = filterSet.Add("(" + notificationFilter + ")");
            }

            if (notificationReportRequest.TrackingIdsFilter?.Count > 0)
            {
                foreach (var item in notificationReportRequest.TrackingIdsFilter)
                {
                    trackingIdFilter = trackingIdFilter == null ? $"TrackingId eq '{item}'" : trackingIdFilter + " or " + $"TrackingId eq '{item}'";
                }

                _ = filterSet.Add("(" + trackingIdFilter + ")");
            }

            if (notificationReportRequest.NotificationStatusFilter?.Count > 0)
            {
                foreach (int item in notificationReportRequest.NotificationStatusFilter)
                {
                    string status = GetStatus(item);
                    statusFilter = statusFilter == null ? $"Status eq '{status}'" : statusFilter + " or " + $"Status eq '{status}'";
                }

                _ = filterSet.Add("(" + statusFilter + ")");
            }

            filterExpression = PrepareFilterExp(filterSet);
            return filterExpression;

            static string PrepareFilterExp(HashSet<string> filterSet)
            {
                string filterExp = string.Join(" and ", filterSet.ToArray());
                return filterExp;
            }
        }

        private string GetDateFilterExpression(NotificationReportRequest notificationReportRequest)
        {
            string filterExpression = null;
            if (DateTime.TryParse(notificationReportRequest.CreatedDateTimeStart, out DateTime createdDateTimeStart))
            {
                filterExpression = filterExpression == null ? $"CreatedDateTimeStart ge {GenerateFilterForDate(createdDateTimeStart)}" : filterExpression + " and " + $"CreatedDateTimeStart ge {GenerateFilterForDate(createdDateTimeStart)}";
            }

            if (DateTime.TryParse(notificationReportRequest.CreatedDateTimeEnd, out DateTime createdDateTimeEnd))
            {
                filterExpression = filterExpression == null ? $" CreatedDateTimeEnd le {GenerateFilterForDate(createdDateTimeEnd)}" : filterExpression + " and " + $"CreatedDateTimeEnd le {GenerateFilterForDate(createdDateTimeEnd)}";
            }

            if (DateTime.TryParse(notificationReportRequest.SendOnUtcDateStart, out DateTime sentTimeStart))
            {
                filterExpression = filterExpression == null ? $" SendOnUtcDate ge {GenerateFilterForDate(sentTimeStart)}" : filterExpression + " and " + $"SendOnUtcDate ge {GenerateFilterForDate(sentTimeStart)}";
            }

            if (DateTime.TryParse(notificationReportRequest.SendOnUtcDateEnd, out DateTime sentTimeEnd))
            {
                filterExpression = filterExpression == null ? $" SendOnUtcDate le {GenerateFilterForDate(sentTimeEnd)}" : filterExpression + " and " + $"SendOnUtcDate le {GenerateFilterForDate(sentTimeEnd)}";
            }

            if (DateTime.TryParse(notificationReportRequest.UpdatedDateTimeStart, out DateTime updatedTimeStart))
            {
                filterExpression = filterExpression == null ? $" UpdatedDateTimeStart ge {GenerateFilterForDate(updatedTimeStart)}" : filterExpression + " and " + $"UpdatedDateTimeStart ge {GenerateFilterForDate(updatedTimeStart)}";
            }

            if (DateTime.TryParse(notificationReportRequest.UpdatedDateTimeEnd, out DateTime updatedTimeEnd))
            {
                filterExpression = filterExpression == null ? $" UpdatedDateTimeEnd le {GenerateFilterForDate(updatedTimeEnd)}" : filterExpression + " and " + $"UpdatedDateTimeEnd le {GenerateFilterForDate(updatedTimeEnd)}";
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

        private string GenerateFilterForDate(DateTimeOffset filterDate)
        {
            return string.Format(CultureInfo.InvariantCulture, "datetime'{0}'", filterDate.UtcDateTime.ToString("o", CultureInfo.InvariantCulture));
        }

        private string CombineFilters(string filterOne, string operatorString, string filterTwo)
        {
            return string.Format(CultureInfo.InvariantCulture, "({0}) {1} ({2})", filterOne, operatorString, filterTwo);
        }
    }
}
