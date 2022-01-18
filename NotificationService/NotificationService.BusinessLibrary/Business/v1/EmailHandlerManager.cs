// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Business.V1
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models;
    using NotificationService.Contracts.Models.Request;
    using NotificationService.Data;

    /// <summary>
    /// Business Manager for Notification Handler.
    /// </summary>
    public class EmailHandlerManager : IEmailHandlerManager
    {
        /// <summary>
        /// MS Graph configuration.
        /// </summary>
        private readonly MSGraphSetting mSGraphSetting;

        /// <summary>
        /// Instance of Application Configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Instance of <see cref="ICloudStorageClient"/>.
        /// </summary>
        private readonly ICloudStorageClient cloudStorageClient;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of <see cref="IEmailManager"/>.
        /// </summary>
        private readonly IEmailManager emailManager;

        /// <summary>
        /// StorageAccountSetting configuration object.
        /// </summary>
        private readonly string notificationQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailHandlerManager"/> class.
        /// </summary>
        /// <param name="configuration">An instance of <see cref="IConfiguration"/>.</param>
        /// <param name="mSGraphSetting">Graph settings  <see cref="MSGraphSetting"/>.</param>
        /// <param name="cloudStorageClient">An instance of <see cref="ICloudStorageClient"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        /// <param name="emailManager">An instance of <see cref="IEmailManager"/>.</param>
        public EmailHandlerManager(
            IConfiguration configuration,
            IOptions<MSGraphSetting> mSGraphSetting,
            ICloudStorageClient cloudStorageClient,
            ILogger logger,
            IEmailManager emailManager)
        {
            this.configuration = configuration;
            this.mSGraphSetting = mSGraphSetting?.Value;
            this.cloudStorageClient = cloudStorageClient;
            this.logger = logger;
            this.emailManager = emailManager;
            this.notificationQueue = this.configuration?[$"{ConfigConstants.StorageAccountConfigSectionKey}:{ConfigConstants.StorageAccNotificationQueueName}"];
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationResponse>> QueueEmailNotifications(string applicationName, EmailNotificationItem[] emailNotificationItems)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var traceProps = new Dictionary<string, string>();
            bool result = false;
            try
            {
                this.logger.TraceInformation($"Started {nameof(this.QueueEmailNotifications)} method of {nameof(EmailHandlerManager)}.", traceProps);
                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
                }

                if (emailNotificationItems is null)
                {
                    throw new ArgumentNullException(nameof(emailNotificationItems));
                }

                traceProps[AIConstants.Application] = applicationName;
                traceProps[AIConstants.EmailNotificationCount] = emailNotificationItems.Length.ToString(CultureInfo.InvariantCulture);

                this.logger.WriteCustomEvent("QueueEmailNotifications Started", traceProps);
                IList<NotificationResponse> notificationResponses = new List<NotificationResponse>();
                IList<EmailNotificationItemEntity> notificationItemEntities = await this.emailManager.CreateNotificationEntities(applicationName, emailNotificationItems, NotificationItemStatus.Queued).ConfigureAwait(false);
                List<List<EmailNotificationItemEntity>> entitiesToQueue;
                if (string.Equals(this.configuration?[ConfigConstants.NotificationProviderType], NotificationProviderType.Graph.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    entitiesToQueue = BusinessUtilities.SplitList<EmailNotificationItemEntity>(notificationItemEntities.ToList(), this.mSGraphSetting.BatchRequestLimit).ToList();
                }
                else
                {
                    entitiesToQueue = new List<List<EmailNotificationItemEntity>> { notificationItemEntities.ToList() };
                }

                // Queue a single cloud message for all entities created to enable parallel processing.
                var cloudQueue = this.cloudStorageClient.GetCloudQueue(this.notificationQueue);

                foreach (var item in entitiesToQueue)
                {
                    this.logger.TraceVerbose($"Started {nameof(BusinessUtilities.GetCloudMessagesForEntities)} method of {nameof(EmailHandlerManager)}.", traceProps);
                    IList<string> cloudMessages = BusinessUtilities.GetCloudMessagesForEntities(applicationName, item);
                    this.logger.TraceVerbose($"Completed {nameof(BusinessUtilities.GetCloudMessagesForEntities)} method of {nameof(EmailHandlerManager)}.", traceProps);

                    this.logger.TraceVerbose($"Started {nameof(this.cloudStorageClient.QueueCloudMessages)} method of {nameof(EmailHandlerManager)}.", traceProps);
                    await this.cloudStorageClient.QueueCloudMessages(cloudQueue, cloudMessages).ConfigureAwait(false);
                    this.logger.TraceVerbose($"Completed {nameof(this.cloudStorageClient.QueueCloudMessages)} method of {nameof(EmailHandlerManager)}.", traceProps);
                }

                var responses = this.emailManager.NotificationEntitiesToResponse(notificationResponses, notificationItemEntities);
                this.logger.TraceInformation($"Completed {nameof(this.QueueEmailNotifications)} method of {nameof(EmailHandlerManager)}.", traceProps);
                result = true;
                return responses;
            }
            catch (Exception e)
            {
                this.logger.WriteException(e, traceProps);
                result = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                traceProps[AIConstants.Result] = result.ToString(CultureInfo.InvariantCulture);
                var metrics = new Dictionary<string, double>();
                metrics[AIConstants.Duration] = stopwatch.ElapsedMilliseconds;
                this.logger.WriteCustomEvent("QueueEmailNotifications Completed", traceProps, metrics);
            }
        }

        /// <summary>
        /// Queue email notification items.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="meetingNotificationItems">Array of email notification items.</param>
        /// <returns>
        /// A <see cref="Task" /> representing the result of the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentException">Application Name cannot be null or empty. - applicationName.</exception>
        /// <exception cref="ArgumentNullException">meetingNotificationItems.</exception>
        public async Task<IList<NotificationResponse>> QueueMeetingNotifications(string applicationName, MeetingNotificationItem[] meetingNotificationItems)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var traceProps = new Dictionary<string, string>();
            bool result = false;
            try
            {
                this.logger.TraceInformation($"Started {nameof(this.QueueMeetingNotifications)} method of {nameof(EmailHandlerManager)}.", traceProps);
                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
                }

                if (meetingNotificationItems is null)
                {
                    throw new ArgumentNullException(nameof(meetingNotificationItems));
                }

                traceProps[AIConstants.Application] = applicationName;
                traceProps[AIConstants.MeetingNotificationCount] = meetingNotificationItems.Length.ToString(CultureInfo.InvariantCulture);

                this.logger.WriteCustomEvent("QueueEmailNotifications Started", traceProps);
                IList<NotificationResponse> notificationResponses = new List<NotificationResponse>();
                IList<MeetingNotificationItemEntity> notificationItemEntities = await this.emailManager.CreateMeetingNotificationEntities(applicationName, meetingNotificationItems, NotificationItemStatus.Queued).ConfigureAwait(false);
                List<List<MeetingNotificationItemEntity>> entitiesToQueue;
                if (string.Equals(this.configuration?[ConfigConstants.NotificationProviderType], NotificationProviderType.Graph.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    entitiesToQueue = BusinessUtilities.SplitList<MeetingNotificationItemEntity>(notificationItemEntities.ToList(), this.mSGraphSetting.BatchRequestLimit).ToList();
                }
                else
                {
                    entitiesToQueue = new List<List<MeetingNotificationItemEntity>> { notificationItemEntities.ToList() };
                }

                // Queue a single cloud message for all entities created to enable parallel processing.
                var cloudQueue = this.cloudStorageClient.GetCloudQueue(this.notificationQueue);

                foreach (var item in entitiesToQueue)
                {
                    this.logger.TraceVerbose($"Started {nameof(BusinessUtilities.GetCloudMessagesForEntities)} method of {nameof(EmailHandlerManager)}.", traceProps);
                    IList<string> cloudMessages = BusinessUtilities.GetCloudMessagesForEntities(applicationName, item);
                    this.logger.TraceVerbose($"Completed {nameof(BusinessUtilities.GetCloudMessagesForEntities)} method of {nameof(EmailHandlerManager)}.", traceProps);

                    this.logger.TraceVerbose($"Started {nameof(this.cloudStorageClient.QueueCloudMessages)} method of {nameof(EmailHandlerManager)}.", traceProps);
                    await this.cloudStorageClient.QueueCloudMessages(cloudQueue, cloudMessages).ConfigureAwait(false);
                    this.logger.TraceVerbose($"Completed {nameof(this.cloudStorageClient.QueueCloudMessages)} method of {nameof(EmailHandlerManager)}.", traceProps);
                }

                var responses = this.emailManager.NotificationEntitiesToResponse(notificationResponses, notificationItemEntities);
                this.logger.TraceInformation($"Completed {nameof(this.QueueMeetingNotifications)} method of {nameof(EmailHandlerManager)}.", traceProps);
                result = true;
                return responses;
            }
            catch (Exception e)
            {
                this.logger.WriteException(e, traceProps);
                result = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                traceProps[AIConstants.Result] = result.ToString(CultureInfo.InvariantCulture);
                var metrics = new Dictionary<string, double>();
                metrics[AIConstants.Duration] = stopwatch.ElapsedMilliseconds;
                this.logger.WriteCustomEvent("QueueEmailNotifications Completed", traceProps, metrics);
            }
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationResponse>> ResendNotifications(string applicationName, string[] notificationIds, NotificationType notifType = NotificationType.Mail, bool ignoreAlreadySent = false)
        {
            this.logger.TraceInformation($"Started {nameof(this.ResendNotifications)} method of {nameof(EmailHandlerManager)}.");
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
            }

            if (notificationIds is null)
            {
                throw new ArgumentNullException(nameof(notificationIds));
            }

            IList<NotificationResponse> notificationResponses = new List<NotificationResponse>();

            // Queue a single cloud message for all entities created to enable parallel processing.
            var cloudQueue = this.cloudStorageClient.GetCloudQueue(this.notificationQueue);
            IList<string> cloudMessages = BusinessUtilities.GetCloudMessagesForIds(applicationName, notificationIds, notifType, ignoreAlreadySent);
            await this.cloudStorageClient.QueueCloudMessages(cloudQueue, cloudMessages).ConfigureAwait(false);

            notificationIds.ToList().ForEach(id =>
            {
                notificationResponses.Add(new NotificationResponse()
                {
                    NotificationId = id,
                    Status = NotificationItemStatus.Queued,
                });
            });
            this.logger.TraceInformation($"Finished {nameof(this.ResendNotifications)} method of {nameof(EmailHandlerManager)}.");
            return notificationResponses;
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationResponse>> ResendEmailNotificationsByDateRange(string applicationName, DateTimeRange dateRange)
        {
            this.logger.TraceInformation($"Started {nameof(this.ResendEmailNotificationsByDateRange)} method of {nameof(EmailHandlerManager)}.");
            var allowedMaxResendDurationInDays = (double)this.configuration.GetValue(typeof(double), ConfigConstants.AllowedMaxResendDurationInDays);
            if (dateRange != null && (dateRange.EndDate - dateRange.StartDate).TotalDays >= allowedMaxResendDurationInDays)
            {
                throw new DataException($"Date-range must not be less or equal to {allowedMaxResendDurationInDays}");
            }

            var statusList = new List<NotificationItemStatus>() { NotificationItemStatus.Failed };
            var failedNotificationEntities = await this.emailManager.GetEmailNotificationsByDateRangeAndStatus(applicationName, dateRange, statusList).ConfigureAwait(false);
            if (failedNotificationEntities == null || failedNotificationEntities.Count == 0)
            {
                return null;
            }

            var notificationIds = failedNotificationEntities.Select(notificationEntity => notificationEntity.NotificationId);
            var result = await this.ResendNotifications(applicationName, notificationIds.ToArray(), NotificationType.Mail, true).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.ResendEmailNotificationsByDateRange)} method of {nameof(EmailHandlerManager)}.");
            return result;
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationResponse>> ResendMeetingNotificationsByDateRange(string applicationName, DateTimeRange dateRange)
        {
            this.logger.TraceInformation($"Started {nameof(this.ResendMeetingNotificationsByDateRange)} method of {nameof(EmailHandlerManager)}.");
            var allowedMaxResendDurationInDays = (double)this.configuration.GetValue(typeof(double), ConfigConstants.AllowedMaxResendDurationInDays);
            if (dateRange != null && (dateRange.EndDate - dateRange.StartDate).TotalDays >= allowedMaxResendDurationInDays)
            {
                throw new DataException($"Date-range must not be less or equal to {allowedMaxResendDurationInDays}");
            }

            var statusList = new List<NotificationItemStatus>() { NotificationItemStatus.Failed };
            var failedNotificationEntities = await this.emailManager.GetMeetingNotificationsByDateRangeAndStatus(applicationName, dateRange, statusList).ConfigureAwait(false);
            if (failedNotificationEntities == null || failedNotificationEntities.Count == 0)
            {
                return null;
            }

            var notificationIds = failedNotificationEntities.Select(notificationEntity => notificationEntity.NotificationId);
            var result = await this.ResendNotifications(applicationName, notificationIds.ToArray(), NotificationType.Meet, true).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.ResendMeetingNotificationsByDateRange)} method of {nameof(EmailHandlerManager)}.");
            return result;
        }
    }
}
