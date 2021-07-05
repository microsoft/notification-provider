// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationsQueueProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureAppConfiguration;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Newtonsoft.Json;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Data;
    using NotificationService.Data.Interfaces;

    /// <summary>
    /// Function to process notification queue items.
    /// </summary>
    public class ProcessNotificationQueueItem
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of <see cref="IEmailNotificationRepository"/>.
        /// </summary>
        private readonly IEmailNotificationRepository emailNotificationRepository;

        /// <summary>
        /// Instance of Application Configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Instance of HttpClientHelper for making http api calls.
        /// </summary>
        private readonly IHttpClientHelper httpClientHelper;

        /// <summary>
        /// Enum to specify type of database.
        /// </summary>
        private readonly StorageType repo;

        /// <summary>
        /// Instance of <see cref="IConfigurationRefresher"/>.
        /// </summary>
        private readonly IConfigurationRefresher configurationRefresher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessNotificationQueueItem"/> class.
        /// </summary>
        /// <param name="logger">The log.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="repositoryFactory">The repositoryFactory.</param>
        /// <param name="httpClientHelper">The httpClientHelper.</param>
        /// <param name="refresherProvider">The IConfigurationRefresherProvider.</param>
        public ProcessNotificationQueueItem(
            ILogger logger,
            IConfiguration configuration,
            IRepositoryFactory repositoryFactory,
            IHttpClientHelper httpClientHelper,
            IConfigurationRefresherProvider refresherProvider)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.emailNotificationRepository = repositoryFactory.GetRepository(Enum.TryParse<StorageType>(this.configuration?[Constants.StorageType], out this.repo) ? this.repo : throw new Exception());
            this.httpClientHelper = httpClientHelper;
            this.configurationRefresher = refresherProvider.Refreshers.First();

            //No need to await
            RefreshKeys();
        }

        /// <summary>
        /// Refresh the azure app configuration.
        /// </summary>
        private async Task RefreshKeys()
        {
            _ = await this.configurationRefresher.TryRefreshAsync();
        }

        /// <summary>
        /// Trigger method invoked when a notification item is added to the queue.
        /// </summary>
        /// <param name="inputQueueItem">Serialized queue item.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        [FunctionName("ProcessNotificationQueueItem")]
        public async Task Run([QueueTrigger("%NotificationQueueName%", Connection = "AzureWebJobsStorage")] CloudQueueMessage inputQueueItem)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var notifQueueItem = inputQueueItem.AsString;
            var traceProps = new Dictionary<string, string>();
            traceProps["DequeueCount"] = inputQueueItem.DequeueCount.ToString();
            traceProps["QueueMessageId"] = inputQueueItem.Id;
            traceProps["InsertionTime"] = inputQueueItem.InsertionTime.ToString();
            this.logger.TraceInformation($"ProcessNotificationQueueItem started processing: {notifQueueItem}");
            QueueNotificationItem queueNotificationItem = null;
            try
            {
                if (string.IsNullOrEmpty(notifQueueItem))
                {
                    throw new ArgumentException("message", nameof(notifQueueItem));
                }

                queueNotificationItem = JsonConvert.DeserializeObject<QueueNotificationItem>(notifQueueItem);

                traceProps[AIConstants.Application] = queueNotificationItem?.Application;
                traceProps[AIConstants.CorrelationId] = queueNotificationItem?.CorrelationId;
                traceProps[AIConstants.NotificationIds] = string.Join(',', queueNotificationItem?.NotificationIds);
                traceProps[AIConstants.EmailNotificationCount] = string.Join(',', queueNotificationItem?.NotificationIds?.Length);
                traceProps[AIConstants.NotificationType] = queueNotificationItem?.NotificationType.ToString();
                this.logger.TraceInformation($"ProcessNotificationQueueItem. Notification Item: {notifQueueItem}", traceProps);
                this.logger.WriteCustomEvent("QueueEmailNotifications Started", traceProps);

                _ = traceProps.Remove(AIConstants.NotificationIds);
                if (queueNotificationItem != null)
                {
                    var notifType = queueNotificationItem.NotificationType == NotificationType.Mail ? Constants.EmailNotificationType : Constants.MeetingNotificationType;

                    var stringContent = new StringContent(JsonConvert.SerializeObject(queueNotificationItem), Encoding.UTF8, Constants.JsonMIMEType);
                    string notificationServiceEndpoint = this.configuration?[Constants.NotificationServiceEndpoint];
                    this.logger.TraceVerbose($"ProcessNotificationQueueItem fetching token to call notification service endpoint...", traceProps);

                    this.logger.TraceInformation($"ProcessNotificationQueueItem calling notification service endpoint...", traceProps);
                    var response = await this.httpClientHelper.PostAsync($"{notificationServiceEndpoint}/v1/{notifType}/process/{queueNotificationItem.Application}", stringContent);
                    this.logger.TraceInformation($"ProcessNotificationQueueItem received response from notification service endpoint.", traceProps);
                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        this.logger.WriteException(new Exception($"An error occurred while processing {notifQueueItem} in ProcessNotificationQueueItem. Details: [StatusCode = {response.StatusCode}, Content = {content}]."), traceProps);
                    }
                }
                else
                {
                    this.logger.WriteException(new Exception("Invalid queue item received by the processor."), traceProps);
                }
            }
            catch (TaskCanceledException ex)
            {
                this.logger.WriteException(ex, traceProps);

                // Ignoring this exception as notification service would process items but http has been timedout here.
            }
            catch (Exception ex)
            {
                string maxDequeueCountVal = Startup.MaxDequeueCount.Value;

                int maxDeqCount;
                if (int.TryParse(maxDequeueCountVal, out maxDeqCount))
                {
                    if (inputQueueItem.DequeueCount < maxDeqCount)
                    {
                        this.logger.WriteException(ex, traceProps);
                        throw;
                    }
                }

                this.logger.WriteException(ex, traceProps);
                await this.UpdateStatusOfNotificationItemsAsync(queueNotificationItem.NotificationIds, NotificationItemStatus.Failed, ex.Message);
            }
            finally
            {
                this.logger.TraceInformation($"ProcessNotificationQueueItem finished processing: {notifQueueItem}", traceProps);
                stopwatch.Stop();

                var metrics = new Dictionary<string, double>();
                metrics[AIConstants.Duration] = stopwatch.ElapsedMilliseconds;
                this.logger.WriteCustomEvent("QueueEmailNotifications Completed", traceProps, metrics);
            }
        }

        private async Task UpdateStatusOfNotificationItemsAsync(string[] notificationIds, NotificationItemStatus status, string errorMessage)
        {
            this.logger.TraceInformation($"UpdateStatusOfNotificationItemsAsync started. Notification Ids: {notificationIds}");
            try
            {
                IList<EmailNotificationItemEntity> notificationEntities = await this.emailNotificationRepository.GetEmailNotificationItemEntities(new List<string>(notificationIds)).ConfigureAwait(false);
                if (notificationEntities.Count == 0)
                {
                    this.logger.WriteException(new ArgumentException("No records found for the input notification ids.", nameof(notificationIds)));
                }

                foreach (var item in notificationEntities)
                {
                    item.Status = status;
                    item.ErrorMessage = errorMessage;
                }

                // Update the status of processed entities
                await this.emailNotificationRepository.UpdateEmailNotificationItemEntities(notificationEntities).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
            }

            this.logger.TraceInformation($"UpdateStatusOfNotificationItemsAsync finished.");
        }
    }
}