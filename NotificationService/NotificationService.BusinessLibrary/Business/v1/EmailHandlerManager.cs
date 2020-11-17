// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Business.v1
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Data;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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

                traceProps[Constants.Application] = applicationName;
                traceProps[Constants.EmailNotificationCount] = emailNotificationItems.Length.ToString();

                this.logger.WriteCustomEvent("QueueEmailNotifications Started", traceProps);
                IList<NotificationResponse> notificationResponses = new List<NotificationResponse>();
                this.logger.TraceVerbose($"Started {nameof(this.emailManager.CreateNotificationEntities)} method of {nameof(EmailHandlerManager)}.", traceProps);
                IList<EmailNotificationItemEntity> notificationItemEntities = await this.emailManager.CreateNotificationEntities(applicationName, emailNotificationItems, NotificationItemStatus.Queued).ConfigureAwait(false);
                this.logger.TraceVerbose($"Completed {nameof(this.emailManager.CreateNotificationEntities)} method of {nameof(EmailHandlerManager)}.", traceProps);
                List<List<EmailNotificationItemEntity>> entitiesToQueue;
                if (string.Equals(this.configuration?[Constants.NotificationProviderType], NotificationProviderType.Graph))
                {
                    entitiesToQueue = BusinessUtilities.SplitList<EmailNotificationItemEntity>(notificationItemEntities.ToList(), this.mSGraphSetting.BatchRequestLimit).ToList();
                }
                else
                {
                    entitiesToQueue = new List<List<EmailNotificationItemEntity>> { notificationItemEntities.ToList() };
                }

                // Queue a single cloud message for all entities created to enable parallel processing.
                var cloudQueue = this.cloudStorageClient.GetCloudQueue("notifications-queue");

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
                traceProps[Constants.Result] = result.ToString();
                var metrics = new Dictionary<string, double>();
                metrics[Constants.Duration] = stopwatch.ElapsedMilliseconds;
                this.logger.WriteCustomEvent("QueueEmailNotifications Completed", traceProps, metrics);
            }
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationResponse>> ResendEmailNotifications(string applicationName, string[] notificationIds)
        {
            this.logger.TraceInformation($"Started {nameof(this.ResendEmailNotifications)} method of {nameof(EmailHandlerManager)}.");
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
            var cloudQueue = this.cloudStorageClient.GetCloudQueue("notifications-queue");
            IList<string> cloudMessages = BusinessUtilities.GetCloudMessagesForIds(applicationName, notificationIds, false);
            await this.cloudStorageClient.QueueCloudMessages(cloudQueue, cloudMessages).ConfigureAwait(false);

            notificationIds.ToList().ForEach(id =>
            {
                notificationResponses.Add(new NotificationResponse()
                {
                    NotificationId = id,
                    Status = NotificationItemStatus.Queued,
                });
            });
            this.logger.TraceInformation($"Finished {nameof(this.ResendEmailNotifications)} method of {nameof(EmailHandlerManager)}.");
            return notificationResponses;
        }
    }
}
