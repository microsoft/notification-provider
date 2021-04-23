// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Business.V1
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Models;
    using NotificationService.Data;
    using NotificationService.Data.Interfaces;

    /// <summary>
    /// Business Manager for Notification Service.
    /// </summary>
    public class EmailServiceManager : IEmailServiceManager
    {
        /// <summary>
        /// Instance of Application Configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Instance of <see cref="IEmailNotificationRepository"/>.
        /// </summary>
        private readonly IEmailNotificationRepository emailNotificationRepository;

        /// <summary>
        /// Instance of <see cref="ICloudStorageClient"/>.
        /// </summary>
        private readonly ICloudStorageClient cloudStorageClient;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Gets the MailSettings confiured.
        /// </summary>
        private readonly List<MailSettings> mailSettings;

        /// <summary>
        /// Enum to specify type of database.
        /// </summary>
        private readonly StorageType repo;

        /// <summary>
        /// Instance of <see cref="INotificationProvider"/>.
        /// </summary>
        private readonly INotificationProvider notificationProvider;

        /// <summary>
        /// Enum to specify type of Notification Provider.
        /// </summary>
        private readonly NotificationProviderType provider;

        /// <summary>
        /// Instance of <see cref="IEmailManager"/>.
        /// </summary>
        private readonly IEmailManager emailManager;

        /// <summary>
        /// StorageAccountSetting configuration object.
        /// </summary>
        private readonly string notificationQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailServiceManager"/> class.
        /// </summary>
        /// <param name="configuration">An instance of <see cref="IConfiguration"/>.</param>
        /// <param name="repositoryFactory">An instance of <see cref="IRepositoryFactory"/>.</param>
        /// <param name="cloudStorageClient">An instance of <see cref="ICloudStorageClient"/>.</param>
        /// <param name="logger">Instance of Logger.</param>
        /// <param name="notificationProviderFactory">An instance of <see cref="INotificationProviderFactory"/>.</param>
        /// <param name="emailManager">An instance of <see cref="IEmailManager"/>.</param>
        public EmailServiceManager(
            IConfiguration configuration,
            IRepositoryFactory repositoryFactory,
            ICloudStorageClient cloudStorageClient,
            ILogger logger,
            INotificationProviderFactory notificationProviderFactory,
            IEmailManager emailManager)
        {
            this.configuration = configuration;
            this.emailNotificationRepository = repositoryFactory?.GetRepository(Enum.TryParse<StorageType>(this.configuration?[ConfigConstants.StorageType], out this.repo) ? this.repo : throw new Exception());
            this.cloudStorageClient = cloudStorageClient;
            this.logger = logger;
            this.notificationProvider = notificationProviderFactory?.GetNotificationProvider(Enum.TryParse<NotificationProviderType>(this.configuration?[ConfigConstants.NotificationProviderType], out this.provider) ? this.provider : throw new Exception());
            if (this.configuration?[ConfigConstants.MailSettingsConfigKey] != null)
            {
                this.mailSettings = JsonConvert.DeserializeObject<List<MailSettings>>(this.configuration?[ConfigConstants.MailSettingsConfigKey]);
            }

            this.notificationQueue = this.configuration?[$"{ConfigConstants.StorageAccountConfigSectionKey}:{ConfigConstants.StorageAccNotificationQueueName}"];
            this.emailManager = emailManager;
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationResponse>> SendEmailNotifications(string applicationName, EmailNotificationItem[] emailNotificationItems)
        {
            this.logger.TraceInformation($"Started {nameof(this.SendEmailNotifications)} method of {nameof(EmailServiceManager)}.");
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
            }

            if (emailNotificationItems is null)
            {
                throw new ArgumentNullException(nameof(emailNotificationItems));
            }

            IList<NotificationResponse> notificationResponses = new List<NotificationResponse>();
            IList<EmailNotificationItemEntity> notificationItemEntities = await this.SendNotificationsUsingProvider(applicationName, emailNotificationItems).ConfigureAwait(false);
            var responses = this.emailManager.NotificationEntitiesToResponse(notificationResponses, notificationItemEntities);
            this.logger.TraceInformation($"Finished {nameof(this.SendEmailNotifications)} method of {nameof(EmailServiceManager)}.");
            return responses;
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationResponse>> ProcessEmailNotifications(string applicationName, QueueNotificationItem queueNotificationItem)
        {
            var traceprops = new Dictionary<string, string>();
            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.EmailNotificationCount] = queueNotificationItem?.NotificationIds.Length.ToString(CultureInfo.InvariantCulture);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            bool result = false;
            this.logger.WriteCustomEvent("ProcessEmailNotifications Started", traceprops);
            try
            {
                this.logger.TraceInformation($"Started {nameof(this.ProcessEmailNotifications)} method of {nameof(EmailServiceManager)}.");
                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
                }

                if (queueNotificationItem is null)
                {
                    throw new ArgumentNullException(nameof(queueNotificationItem));
                }

                IList<NotificationResponse> notificationResponses = new List<NotificationResponse>();
                IList<EmailNotificationItemEntity> notificationItemEntities = await this.ProcessNotificationsUsingProvider(applicationName, queueNotificationItem).ConfigureAwait(false);
                var responses = this.emailManager.NotificationEntitiesToResponse(notificationResponses, notificationItemEntities);
                this.logger.TraceInformation($"Finished {nameof(this.ProcessEmailNotifications)} method of {nameof(EmailServiceManager)}.");
                result = true;
                return responses;
            }
            catch (Exception ex)
            {
                result = true;
                this.logger.WriteException(ex, traceprops);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                traceprops[AIConstants.Result] = result.ToString(CultureInfo.InvariantCulture);
                var metrics = new Dictionary<string, double>();
                metrics[AIConstants.Duration] = stopwatch.ElapsedMilliseconds;
                this.logger.WriteCustomEvent("ProcessEmailNotifications Completed", traceprops, metrics);
            }
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationResponse>> SendMeetingInvites(string applicationName, MeetingNotificationItem[] meetingInviteItems)
        {
            this.logger.TraceInformation($"Started {nameof(this.SendMeetingInvites)} method of {nameof(EmailServiceManager)}.");
            IList<NotificationResponse> notificationResponses = new List<NotificationResponse>();
            IList<MeetingNotificationItemEntity> emailNotificationEntities = await this.emailManager.CreateMeetingNotificationEntities(applicationName, meetingInviteItems, NotificationItemStatus.Processing).ConfigureAwait(false);
            IList<MeetingNotificationItemEntity> notificationEntities = await this.emailNotificationRepository.GetMeetingNotificationItemEntities(emailNotificationEntities.Select(e => e.NotificationId).ToList(), applicationName).ConfigureAwait(false);
            var notificationItemEntities = await this.ProcessMeetingNotificationEntities(applicationName, notificationEntities).ConfigureAwait(false);
            var responses = this.emailManager.NotificationEntitiesToResponse(notificationResponses, notificationItemEntities);
            this.logger.TraceInformation($"Finished {nameof(this.SendMeetingInvites)} method of {nameof(EmailServiceManager)}.");
            return responses;
        }

        /// <inheritdoc/>
        public async Task<IList<NotificationResponse>> ProcessMeetingNotifications(string applicationName, QueueNotificationItem queueNotificationItem)
        {
            var traceprops = new Dictionary<string, string>();
            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.MeetingNotificationCount] = queueNotificationItem?.NotificationIds.Length.ToString(CultureInfo.InvariantCulture);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            bool result = false;
            this.logger.WriteCustomEvent("ProcessMeetingNotifications Started", traceprops);
            try
            {
                this.logger.TraceInformation($"Started {nameof(this.ProcessMeetingNotifications)} method of {nameof(EmailServiceManager)}.");
                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
                }

                if (queueNotificationItem is null)
                {
                    throw new ArgumentNullException(nameof(queueNotificationItem));
                }

                IList<NotificationResponse> notificationResponses = new List<NotificationResponse>();
                IList<MeetingNotificationItemEntity> notificationItemEntities = await this.ProcessMeetingNotificationsUsingProvider(applicationName, queueNotificationItem).ConfigureAwait(false);
                var responses = this.emailManager.NotificationEntitiesToResponse(notificationResponses, notificationItemEntities);
                this.logger.TraceInformation($"Finished {nameof(this.ProcessEmailNotifications)} method of {nameof(EmailServiceManager)}.");
                result = true;
                return responses;
            }
            catch (Exception ex)
            {
                result = true;
                this.logger.WriteException(ex, traceprops);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                traceprops[AIConstants.Result] = result.ToString(CultureInfo.InvariantCulture);
                var metrics = new Dictionary<string, double>();
                metrics[AIConstants.Duration] = stopwatch.ElapsedMilliseconds;
                this.logger.WriteCustomEvent("ProcessEmailNotifications Completed", traceprops, metrics);
            }
        }

        /// <summary>
        /// Creates the entities in database and send email notifications using MS Graph Provider.
        /// </summary>
        /// <param name="applicationName">Application associated with email notifications.</param>
        /// <param name="emailNotificationItems">Array of notifications items to be sent.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task<IList<EmailNotificationItemEntity>> SendNotificationsUsingProvider(string applicationName, EmailNotificationItem[] emailNotificationItems)
        {
            this.logger.TraceInformation($"Started {nameof(this.SendNotificationsUsingProvider)} method of {nameof(EmailServiceManager)}.");
            IList<EmailNotificationItemEntity> emailNotificationEntities = await this.emailManager.CreateNotificationEntities(applicationName, emailNotificationItems, NotificationItemStatus.Processing).ConfigureAwait(false);
            IList<EmailNotificationItemEntity> notificationEntities = await this.emailNotificationRepository.GetEmailNotificationItemEntities(emailNotificationEntities.Select(e => e.NotificationId).ToList(), applicationName).ConfigureAwait(false);
            var retEntities = await this.ProcessNotificationEntities(applicationName, notificationEntities).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.SendNotificationsUsingProvider)} method of {nameof(EmailServiceManager)}.");
            return retEntities;
        }

        /// <summary>
        /// Fetches the records for given notification ids and resends using MS Graph Provider.
        /// </summary>
        /// <param name="applicationName">Application associated with email notifications.</param>
        /// <param name="queueNotificationItem">Queue Notification Entity.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task<IList<EmailNotificationItemEntity>> ProcessNotificationsUsingProvider(string applicationName, QueueNotificationItem queueNotificationItem)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            IList<EmailNotificationItemEntity> notSentEntities = new List<EmailNotificationItemEntity>();

            List<string> notificationIds = queueNotificationItem.NotificationIds.ToList();
            traceProps[AIConstants.NotificationIds] = JsonConvert.SerializeObject(notificationIds);
            this.logger.TraceInformation($"Started {nameof(this.ProcessNotificationsUsingProvider)} method of {nameof(EmailServiceManager)}.", traceProps);
            IList<EmailNotificationItemEntity> notificationEntities = await this.emailNotificationRepository.GetEmailNotificationItemEntities(notificationIds, applicationName).ConfigureAwait(false);

            var notificationEntitiesToBeSkipped = new List<EmailNotificationItemEntity>();
            if (notificationEntities.Count == 0)
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentException("No records found for the input notification ids.", nameof(notificationIds));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            if (queueNotificationItem.IgnoreAlreadySent)
            {
                notificationEntitiesToBeSkipped = notificationEntities.Where(x => x.Status == NotificationItemStatus.Sent).ToList();
                notificationEntities = notificationEntities.Where(x => x.Status != NotificationItemStatus.Sent).ToList();
            }

            if (notificationEntities.Count == 0)
            {
                return notificationEntitiesToBeSkipped;
            }

            var retEntities = await this.ProcessNotificationEntities(applicationName, notificationEntities).ConfigureAwait(false);
            retEntities = retEntities.Concat(notificationEntitiesToBeSkipped).ToList();
            this.logger.TraceInformation($"Completed {nameof(this.ProcessNotificationsUsingProvider)} method of {nameof(EmailServiceManager)}.", traceProps);

            return retEntities;
        }

        /// <summary>
        /// Fetches the records for given notification ids and resends using MS Graph Provider.
        /// </summary>
        /// <param name="applicationName">Application associated with email notifications.</param>
        /// <param name="queueNotificationItem">Queue Notification Entity.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task<IList<MeetingNotificationItemEntity>> ProcessMeetingNotificationsUsingProvider(string applicationName, QueueNotificationItem queueNotificationItem)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            List<string> notificationIds = queueNotificationItem.NotificationIds.ToList();
            traceProps[AIConstants.NotificationIds] = JsonConvert.SerializeObject(notificationIds);
            IList<MeetingNotificationItemEntity> notSentEntities = new List<MeetingNotificationItemEntity>();

            this.logger.TraceInformation($"Started {nameof(this.ProcessMeetingNotificationsUsingProvider)} method of {nameof(EmailServiceManager)}.", traceProps);
            IList<MeetingNotificationItemEntity> notificationEntities = await this.emailNotificationRepository.GetMeetingNotificationItemEntities(notificationIds, applicationName).ConfigureAwait(false);
            var notificationEntitiesToBeSkipped = new List<MeetingNotificationItemEntity>();
            if (notificationEntities?.Count == 0)
            {
                throw new ArgumentException("No records found for the input notification ids.", nameof(queueNotificationItem));
            }

            if (queueNotificationItem.IgnoreAlreadySent)
            {
                notificationEntitiesToBeSkipped = notificationEntities.Where(x => x.Status == NotificationItemStatus.Sent).ToList();
                notificationEntities = notificationEntities.Where(x => x.Status != NotificationItemStatus.Sent).ToList();
            }

            if (notificationEntities?.Count == 0)
            {
                return notificationEntitiesToBeSkipped;
            }

            var retEntities = await this.ProcessMeetingNotificationEntities(applicationName, notificationEntities).ConfigureAwait(false);
            retEntities = retEntities.Concat(notificationEntitiesToBeSkipped).ToList();
            this.logger.TraceInformation($"Completed {nameof(this.ProcessNotificationsUsingProvider)} method of {nameof(EmailServiceManager)}.", traceProps);

            return retEntities;
        }

        /// <summary>
        /// Chooses an account for application, sends the notifications via Graph and returns back the status.
        /// </summary>
        /// <param name="applicationName">Application associated to the notifications.</param>
        /// <param name="notificationEntities">List of notification entities to be processed.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task<IList<EmailNotificationItemEntity>> ProcessNotificationEntities(string applicationName, IList<EmailNotificationItemEntity> notificationEntities)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;

            this.logger.TraceInformation($"Started {nameof(this.ProcessNotificationEntities)} method of {nameof(EmailServiceManager)}.", traceProps);
            if (this.mailSettings is null || this.mailSettings.Any(a => a.ApplicationName == applicationName) is false)
            {
                this.logger.TraceInformation($"ApplicationName is not present in MailSettings {nameof(this.ProcessNotificationEntities)} method of {nameof(EmailServiceManager)}.", traceProps);
                foreach (var item in notificationEntities)
                {
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = $"ApplicationName is not present in MailSettings.";
                }
            }
            else
            {
                var mailOn = this.mailSettings.Find(a => a.ApplicationName == applicationName).MailOn;
                var sendForReal = this.mailSettings.Find(a => a.ApplicationName == applicationName).SendForReal;
                var toOverride = this.mailSettings.Find(a => a.ApplicationName == applicationName).ToOverride;

                if (!mailOn)
                {
                    this.logger.TraceInformation($"mailOn is set to {mailOn} {nameof(this.ProcessNotificationEntities)} method of {nameof(EmailServiceManager)}.", traceProps);
                    foreach (var item in notificationEntities)
                    {
                        item.Status = NotificationItemStatus.FakeMail;
                        item.ErrorMessage = $"MailOn is set as false for the application:{applicationName}.";
                    }
                }
                else if (!sendForReal && string.IsNullOrEmpty(toOverride))
                {
                    this.logger.TraceInformation($"sendForReal is set to {sendForReal} and toOverride is {toOverride} {nameof(this.ProcessNotificationEntities)} method of {nameof(EmailServiceManager)}.", traceProps);
                    foreach (var item in notificationEntities)
                    {
                        item.Status = NotificationItemStatus.Failed;
                        item.ErrorMessage = $"sendForReal is set to {sendForReal} and toOverride is null or empty for the application:{applicationName}.";
                    }
                }
                else
                {
                    try
                    {
                        await this.notificationProvider.ProcessNotificationEntities(applicationName, notificationEntities).ConfigureAwait(false);
                    }
                    catch (ArgumentNullException ex)
                    {
                        foreach (var item in notificationEntities)
                        {
                            item.Status = NotificationItemStatus.Failed;
                            item.ErrorMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                        }
                    }
                }
            }

            // Update the status of processed entities
            await this.emailNotificationRepository.UpdateEmailNotificationItemEntities(notificationEntities).ConfigureAwait(false);

            // Requeue items that were updated as Queued, due to transient failures
            var retryItemsToBeQueued = notificationEntities?.Where(nie => nie.Status == NotificationItemStatus.Retrying)?.ToList();

            if (retryItemsToBeQueued?.Count > 0)
            {
                this.logger.TraceVerbose("Fetching Cloud Queue", traceProps);
                var cloudQueue = this.cloudStorageClient.GetCloudQueue(this.notificationQueue);
                this.logger.TraceVerbose("Cloud Queue Fetched", traceProps);

                this.logger.TraceVerbose($"Items to be retried exists. Re-queuing. Count:{retryItemsToBeQueued?.Count}", traceProps);
                IList<string> cloudMessages = BusinessUtilities.GetCloudMessagesForEntities(applicationName, retryItemsToBeQueued);
                await this.cloudStorageClient.QueueCloudMessages(cloudQueue, cloudMessages).ConfigureAwait(false);
                this.logger.TraceVerbose($"Items Re-queued. Count:{retryItemsToBeQueued?.Count}", traceProps);
            }

            this.logger.TraceInformation($"Completed {nameof(this.ProcessNotificationEntities)} method of {nameof(EmailServiceManager)}.", traceProps);
            return notificationEntities;
        }

        /// <summary>
        /// Chooses an account for application, sends the notifications via Graph and returns back the status.
        /// </summary>
        /// <param name="applicationName">Application associated to the notifications.</param>
        /// <param name="notificationEntities">List of notification entities to be processed.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task<IList<MeetingNotificationItemEntity>> ProcessMeetingNotificationEntities(string applicationName, IList<MeetingNotificationItemEntity> notificationEntities)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.MeetingNotificationCount] = notificationEntities?.Count.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.ProcessMeetingNotificationEntities)} method of {nameof(EmailServiceManager)}.", traceProps);
            if (this.mailSettings is null || this.mailSettings.Any(a => a.ApplicationName == applicationName) is false)
            {
                this.logger.TraceInformation($"ApplicationName is not present in MailSettings {nameof(this.ProcessMeetingNotificationEntities)} method of {nameof(EmailServiceManager)}.", traceProps);
                foreach (var item in notificationEntities)
                {
                    item.Status = NotificationItemStatus.Failed;
                    item.ErrorMessage = $"ApplicationName is not present in MailSettings.";
                }
            }
            else
            {
                var mailOn = this.mailSettings.Find(a => a.ApplicationName == applicationName).MailOn;
                var sendForReal = this.mailSettings.Find(a => a.ApplicationName == applicationName).SendForReal;
                var toOverride = this.mailSettings.Find(a => a.ApplicationName == applicationName).ToOverride;

                if (!mailOn)
                {
                    this.logger.TraceInformation($"mailOn is set to {mailOn} {nameof(this.ProcessMeetingNotificationEntities)} method of {nameof(EmailServiceManager)}.", traceProps);
                    foreach (var item in notificationEntities)
                    {
                        item.Status = NotificationItemStatus.FakeMail;
                        item.ErrorMessage = $"MailOn is set as false for the application:{applicationName}.";
                    }
                }
                else if (!sendForReal && string.IsNullOrEmpty(toOverride))
                {
                    this.logger.TraceInformation($"sendForReal is set to {sendForReal} and toOverride is {toOverride} {nameof(this.ProcessMeetingNotificationEntities)} method of {nameof(EmailServiceManager)}.", traceProps);
                    foreach (var item in notificationEntities)
                    {
                        item.Status = NotificationItemStatus.Failed;
                        item.ErrorMessage = $"sendForReal is set to {sendForReal} and toOverride is null or empty for the application:{applicationName}.";
                    }
                }
                else
                {
                    try
                    {
                        await this.notificationProvider.ProcessMeetingNotificationEntities(applicationName, notificationEntities).ConfigureAwait(false);
                    }
                    catch (ArgumentNullException ex)
                    {
                        foreach (var item in notificationEntities)
                        {
                            item.Status = NotificationItemStatus.Failed;
                            item.ErrorMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                        }
                    }
                }
            }

            // Update the status of processed entities
            await this.emailNotificationRepository.UpdateMeetingNotificationItemEntities(notificationEntities).ConfigureAwait(false);

            // Requeue items that were updated as Queued, due to transient failures
            var retryItemsToBeQueued = notificationEntities?.Where(nie => nie.Status == NotificationItemStatus.Retrying)?.ToList();

            if (retryItemsToBeQueued?.Count > 0)
            {
                this.logger.TraceVerbose("Fetching Cloud Queue", traceProps);
                var cloudQueue = this.cloudStorageClient.GetCloudQueue(this.notificationQueue);
                this.logger.TraceVerbose("Cloud Queue Fetched", traceProps);

                this.logger.TraceVerbose($"Items to be retried exists. Re-queuing. Count:{retryItemsToBeQueued?.Count.ToString(CultureInfo.InvariantCulture)}", traceProps);
                IList<string> cloudMessages = BusinessUtilities.GetCloudMessagesForEntities(applicationName, retryItemsToBeQueued);
                await this.cloudStorageClient.QueueCloudMessages(cloudQueue, cloudMessages).ConfigureAwait(false);
                this.logger.TraceVerbose($"Items Re-queued. Count:{retryItemsToBeQueued?.Count.ToString(CultureInfo.InvariantCulture)}", traceProps);
            }

            this.logger.TraceInformation($"Completed {nameof(this.ProcessNotificationEntities)} method of {nameof(EmailServiceManager)}.", traceProps);
            return notificationEntities;
        }
    }
}
