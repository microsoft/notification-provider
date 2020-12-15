// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Options;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;

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
        /// Initializes a new instance of the <see cref="TableStorageEmailRepository"/> class.
        /// </summary>
        /// <param name="storageAccountSetting">primary key of storage account.</param>
        /// <param name="cloudStorageClient"> cloud storage client for table storage.</param>
        /// <param name="logger">logger.</param>
        public TableStorageEmailRepository(IOptions<StorageAccountSetting> storageAccountSetting, ITableStorageClient cloudStorageClient, ILogger logger)
        {
            this.storageAccountSetting = storageAccountSetting?.Value ?? throw new System.ArgumentNullException(nameof(storageAccountSetting));
            this.cloudStorageClient = cloudStorageClient ?? throw new System.ArgumentNullException(nameof(cloudStorageClient));
            this.emailHistoryTable = this.cloudStorageClient.GetCloudTable("EmailHistory");
            this.meetingHistoryTable = this.cloudStorageClient.GetCloudTable("MeetingHistory");
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public Task CreateEmailNotificationItemEntities(IList<EmailNotificationItemEntity> emailNotificationItemEntities)
        {
            if (emailNotificationItemEntities is null || emailNotificationItemEntities.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(emailNotificationItemEntities));
            }

            this.logger.TraceInformation($"Started {nameof(this.CreateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");
            TableBatchOperation batchOperation = new TableBatchOperation();
            foreach (var item in emailNotificationItemEntities)
            {
                batchOperation.Insert(this.ConvertToEmailNotificationItemTableEntity(item));
            }

            Task.WaitAll(this.emailHistoryTable.ExecuteBatchAsync(batchOperation));

            this.logger.TraceInformation($"Finished {nameof(this.CreateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<IList<EmailNotificationItemEntity>> GetEmailNotificationItemEntities(IList<string> notificationIds)
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

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");
            List<EmailNotificationItemTableEntity> emailNotificationItemEntities = new List<EmailNotificationItemTableEntity>();
            var linqQuery = new TableQuery<EmailNotificationItemTableEntity>().Where(filterExpression);
            emailNotificationItemEntities = this.emailHistoryTable.ExecuteQuery(linqQuery)?.Select(ent => ent).ToList();
            IList<EmailNotificationItemEntity> notificationEntities = emailNotificationItemEntities.Select(e => this.ConvertToEmailNotificationItemEntity(e)).ToList();
            this.logger.TraceInformation($"Finished {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");
            return Task.FromResult(notificationEntities);
        }

        /// <inheritdoc/>
        public Task<EmailNotificationItemEntity> GetEmailNotificationItemEntity(string notificationId)
        {
            if (notificationId is null)
            {
                throw new System.ArgumentNullException(nameof(notificationId));
            }

            string filterExpression = TableQuery.GenerateFilterCondition("NotificationId", QueryComparisons.Equal, notificationId);
            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(TableStorageEmailRepository)}.");
            List<EmailNotificationItemTableEntity> emailNotificationItemEntities = new List<EmailNotificationItemTableEntity>();
            var linqQuery = new TableQuery<EmailNotificationItemTableEntity>().Where(filterExpression);
            this.logger.TraceInformation($"Finished {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(TableStorageEmailRepository)}.");
            emailNotificationItemEntities = this.emailHistoryTable.ExecuteQuery(linqQuery)?.Select(ent => ent).ToList();
            List<EmailNotificationItemEntity> notificationEntities = emailNotificationItemEntities.Select(e => this.ConvertToEmailNotificationItemEntity(e)).ToList();
            if (emailNotificationItemEntities.Count == 1)
            {
                this.logger.TraceInformation($"Finished {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(EmailNotificationRepository)}.");
                return Task.FromResult(notificationEntities.FirstOrDefault());
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
        public Task<Tuple<IList<EmailNotificationItemEntity>, TableContinuationToken>> GetEmailNotifications(NotificationReportRequest notificationReportRequest)
        {
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

        private string GetFilterExpression(NotificationReportRequest notificationReportRequest)
        {
            string filterExpression = null;
            if (notificationReportRequest.ApplicationFilter?.Count > 0)
            {
                foreach (var item in notificationReportRequest.ApplicationFilter)
                {
                    filterExpression = filterExpression == null ? TableQuery.GenerateFilterCondition("Application", QueryComparisons.Equal, item) : filterExpression + " And " + TableQuery.GenerateFilterCondition("Application", QueryComparisons.Equal, item);
                }
            }

            if (notificationReportRequest.AccountsUsedFilter?.Count > 0)
            {
                foreach (var item in notificationReportRequest.AccountsUsedFilter)
                {
                    filterExpression = filterExpression == null ? TableQuery.GenerateFilterCondition("EmailAccountUsed", QueryComparisons.Equal, item) : filterExpression + " and " + TableQuery.GenerateFilterCondition("EmailAccountUsed", QueryComparisons.Equal, item);
                }
            }

            if (notificationReportRequest.NotificationIdsFilter?.Count > 0)
            {
                foreach (var item in notificationReportRequest.NotificationIdsFilter)
                {
                    filterExpression = filterExpression == null ? TableQuery.GenerateFilterCondition("NotificationId", QueryComparisons.Equal, item) : filterExpression + " and " + TableQuery.GenerateFilterCondition("NotificationId", QueryComparisons.Equal, item);
                }
            }

            if (notificationReportRequest.NotificationStatusFilter?.Count > 0)
            {
                foreach (int item in notificationReportRequest.NotificationStatusFilter)
                {
                    string status = "Queued";
                    switch (item)
                    {
                        case 0:
                            status = "Queued";
                            break;
                        case 1:
                            status = "Processing";
                            break;
                        case 2:
                            status = "Retrying";
                            break;
                        case 3:
                            status = "Failed";
                            break;
                        case 4:
                            status = "Sent";
                            break;
                        case 5:
                            status = "FakeMail";
                            break;
                    }

                    filterExpression = filterExpression == null ? TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal, status) : filterExpression + " and " + TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal, status);
                }
            }

            return filterExpression;
        }

        private EmailNotificationItemTableEntity ConvertToEmailNotificationItemTableEntity(EmailNotificationItemEntity emailNotificationItemEntity)
        {
            EmailNotificationItemTableEntity emailNotificationItemTableEntity = new EmailNotificationItemTableEntity();
            emailNotificationItemTableEntity.PartitionKey = emailNotificationItemEntity.Application;
            emailNotificationItemTableEntity.RowKey = emailNotificationItemEntity.NotificationId;
            emailNotificationItemTableEntity.Application = emailNotificationItemEntity.Application;
            emailNotificationItemTableEntity.Attachments = emailNotificationItemEntity.Attachments;
            emailNotificationItemTableEntity.AttachmentReference = emailNotificationItemEntity.AttachmentReference;
            emailNotificationItemTableEntity.BCC = emailNotificationItemEntity.BCC;
            emailNotificationItemTableEntity.Body = emailNotificationItemEntity.Body;
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
            emailNotificationItemTableEntity.TemplateData = emailNotificationItemEntity.TemplateData;
            emailNotificationItemTableEntity.TemplateName = emailNotificationItemEntity.TemplateName;
            emailNotificationItemTableEntity.Timestamp = emailNotificationItemEntity.Timestamp;
            emailNotificationItemTableEntity.To = emailNotificationItemEntity.To;
            emailNotificationItemTableEntity.TrackingId = emailNotificationItemEntity.TrackingId;
            emailNotificationItemTableEntity.TryCount = emailNotificationItemEntity.TryCount;
            emailNotificationItemTableEntity.ETag = emailNotificationItemEntity.ETag;
            return emailNotificationItemTableEntity;
        }

        private MeetingNotificationItemTableEntity ConvertToMeetingNotificationItemTableEntity(MeetingNotificationItemEntity meetingNotificationItemEntity)
        {
            MeetingNotificationItemTableEntity meetingNotificationItemTableEntity = new MeetingNotificationItemTableEntity();
            meetingNotificationItemTableEntity.PartitionKey = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.RowKey = meetingNotificationItemEntity.NotificationId;
            meetingNotificationItemTableEntity.Application = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.RequiredAttendees = meetingNotificationItemEntity.RequiredAttendees;
            meetingNotificationItemTableEntity.Body = meetingNotificationItemEntity.Body;
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
            meetingNotificationItemTableEntity.TemplateData = meetingNotificationItemEntity.TemplateData;
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
            meetingNotificationItemTableEntity.TemplateData = meetingNotificationItemEntity.TemplateData;
            meetingNotificationItemTableEntity.TemplateName = meetingNotificationItemEntity.TemplateName;
            meetingNotificationItemTableEntity.MeetingEnd = meetingNotificationItemEntity.End;
            meetingNotificationItemTableEntity.MeetingStart = meetingNotificationItemEntity.Start;
            meetingNotificationItemTableEntity.SequenceNumber = meetingNotificationItemEntity.SequenceNumber ?? default;
            meetingNotificationItemTableEntity.SendOnUtcDate = meetingNotificationItemEntity.SendOnUtcDate;
            meetingNotificationItemTableEntity.TrackingId = meetingNotificationItemEntity.TrackingId;
            meetingNotificationItemTableEntity.TryCount = meetingNotificationItemEntity.TryCount;
            meetingNotificationItemTableEntity.Timestamp = meetingNotificationItemEntity.Timestamp;
            meetingNotificationItemTableEntity.RowKey = meetingNotificationItemEntity.NotificationId;
            meetingNotificationItemTableEntity.PartitionKey = meetingNotificationItemEntity.Application;
            meetingNotificationItemTableEntity.Interval = meetingNotificationItemEntity.Interval;
            meetingNotificationItemTableEntity.ICalUid = meetingNotificationItemEntity.ICalUid;
            return meetingNotificationItemTableEntity;
        }

        private MeetingNotificationItemEntity ConvertToMeetingNotificationItemEntity(MeetingNotificationItemTableEntity meetingNotificationItemTableEntity)
        {
            MeetingNotificationItemEntity meetingNotificationItemEntity = new MeetingNotificationItemEntity();
            meetingNotificationItemEntity.Application = meetingNotificationItemTableEntity.Application;
            meetingNotificationItemEntity.NotificationId = meetingNotificationItemTableEntity.NotificationId;
            meetingNotificationItemEntity.RequiredAttendees = meetingNotificationItemTableEntity.RequiredAttendees;
            meetingNotificationItemEntity.Body = meetingNotificationItemTableEntity.Body;
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
            meetingNotificationItemEntity.TemplateData = meetingNotificationItemTableEntity.TemplateData;
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
            meetingNotificationItemEntity.TemplateData = meetingNotificationItemTableEntity.TemplateData;
            meetingNotificationItemEntity.TemplateName = meetingNotificationItemTableEntity.TemplateName;
            meetingNotificationItemEntity.End = meetingNotificationItemTableEntity.MeetingEnd;
            meetingNotificationItemEntity.Start = meetingNotificationItemTableEntity.MeetingStart;
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
            emailNotificationItemEntity.Attachments = emailNotificationItemTableEntity.Attachments;
            emailNotificationItemEntity.AttachmentReference = emailNotificationItemTableEntity.AttachmentReference;
            emailNotificationItemEntity.BCC = emailNotificationItemTableEntity.BCC;
            emailNotificationItemEntity.Body = emailNotificationItemTableEntity.Body;
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
            emailNotificationItemEntity.TemplateData = emailNotificationItemTableEntity.TemplateData;
            emailNotificationItemEntity.TemplateName = emailNotificationItemTableEntity.TemplateName;
            emailNotificationItemEntity.Timestamp = emailNotificationItemTableEntity.Timestamp;
            emailNotificationItemEntity.To = emailNotificationItemTableEntity.To;
            emailNotificationItemEntity.TrackingId = emailNotificationItemTableEntity.TrackingId;
            emailNotificationItemEntity.TryCount = emailNotificationItemTableEntity.TryCount;
            emailNotificationItemEntity.ETag = emailNotificationItemTableEntity.ETag;
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
                filterExpression = filterExpression == null ? TableQuery.GenerateFilterConditionForDate("SendOnUtcDateStart", QueryComparisons.GreaterThanOrEqual, sentTimeStart) : filterExpression + TableQuery.GenerateFilterConditionForDate("SendOnUtcDateStart", QueryComparisons.GreaterThanOrEqual, sentTimeStart);
            }

            if (DateTime.TryParse(notificationReportRequest.SendOnUtcDateEnd, out DateTime sentTimeEnd))
            {
                filterExpression = filterExpression == null ? TableQuery.GenerateFilterConditionForDate("SendOnUtcDateEnd", QueryComparisons.LessThanOrEqual, sentTimeEnd) : filterExpression + TableQuery.GenerateFilterConditionForDate("SendOnUtcDateEnd", QueryComparisons.LessThanOrEqual, sentTimeEnd);
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

        /// <inheritdoc/>
        public Task<IList<MeetingNotificationItemEntity>> GetMeetingNotificationItemEntities(IList<string> notificationIds)
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

            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");
            List<MeetingNotificationItemTableEntity> meetingNotificationItemEntities = new List<MeetingNotificationItemTableEntity>();
            var linqQuery = new TableQuery<MeetingNotificationItemTableEntity>().Where(filterExpression);
            meetingNotificationItemEntities = this.meetingHistoryTable.ExecuteQuery(linqQuery).Select(ent => ent).ToList();
            IList<MeetingNotificationItemEntity> notificationEntities = meetingNotificationItemEntities.Select(e => this.ConvertToMeetingNotificationItemEntity(e)).ToList();
            this.logger.TraceInformation($"Finished {nameof(this.GetEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");
            return Task.FromResult(notificationEntities);
        }

        /// <inheritdoc/>
        public Task<MeetingNotificationItemEntity> GetMeetingNotificationItemEntity(string notificationId)
        {
            if (notificationId is null)
            {
                throw new System.ArgumentNullException(nameof(notificationId));
            }

            string filterExpression = TableQuery.GenerateFilterCondition("NotificationId", QueryComparisons.Equal, notificationId);
            this.logger.TraceInformation($"Started {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(TableStorageEmailRepository)}.");
            List<MeetingNotificationItemTableEntity> meetingNotificationItemEntities = new List<MeetingNotificationItemTableEntity>();
            var linqQuery = new TableQuery<MeetingNotificationItemTableEntity>().Where(filterExpression);
            this.logger.TraceInformation($"Finished {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(TableStorageEmailRepository)}.");
            meetingNotificationItemEntities = this.meetingHistoryTable.ExecuteQuery(linqQuery).Select(ent => ent).ToList();
            List<MeetingNotificationItemEntity> notificationEntities = meetingNotificationItemEntities.Select(e => this.ConvertToMeetingNotificationItemEntity(e)).ToList();
            if (meetingNotificationItemEntities.Count == 1)
            {
                this.logger.TraceInformation($"Finished {nameof(this.GetEmailNotificationItemEntity)} method of {nameof(EmailNotificationRepository)}.");
                return Task.FromResult(notificationEntities.FirstOrDefault());
            }
            else if (meetingNotificationItemEntities.Count > 1)
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
        /// <returns>A <see cref="Task"/>.</returns>
        /// <exception cref="System.ArgumentNullException">meetingNotificationItemEntities.</exception>
        public Task CreateMeetingNotificationItemEntities(IList<MeetingNotificationItemEntity> meetingNotificationItemEntities)
        {
            if (meetingNotificationItemEntities is null || meetingNotificationItemEntities.Count == 0)
            {
                throw new System.ArgumentNullException(nameof(meetingNotificationItemEntities));
            }

            this.logger.TraceInformation($"Started {nameof(this.CreateMeetingNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");
            TableBatchOperation batchOperation = new TableBatchOperation();
            foreach (var item in meetingNotificationItemEntities)
            {
                batchOperation.Insert(this.ConvertToMeetingNotificationItemTableEntity(item));
            }

            Task.WaitAll(this.meetingHistoryTable.ExecuteBatchAsync(batchOperation));

            // TableOperation op = TableOperation.Insert(this.ConvertToMeetingNotificationItemTableEntity(meetingNotificationItemEntities[0]));
            // this.meetingHistoryTable.Execute(op);
            this.logger.TraceInformation($"Finished {nameof(this.CreateEmailNotificationItemEntities)} method of {nameof(TableStorageEmailRepository)}.");

            return Task.FromResult(true);
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
    }
}
