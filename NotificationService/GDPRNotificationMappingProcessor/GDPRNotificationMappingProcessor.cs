// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace GDPRNotificationMappingProcessor
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.OData.Edm.Vocabularies;
    using Newtonsoft.Json;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models.GDPR;
    using NotificationService.Data;
    using NotificationService.Data.Interfaces;

    /// <summary>
    /// Function to process notification messages and create emailId and NotificationId mapping for GDPR scrubbing.
    /// </summary>
    public class GDPRNotificationMappingProcessor
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
        /// Enum to specify type of database.
        /// </summary>
        private readonly StorageType repo;

        /// <summary>
        /// Initializes a new instance of the <see cref="GDPRNotificationMappingProcessor"/> class.
        /// </summary>
        /// <param name="logger">The log.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="repositoryFactory">The repositoryFactory.</param>
        public GDPRNotificationMappingProcessor(
            ILogger logger,
            IConfiguration configuration,
            IRepositoryFactory repositoryFactory)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.emailNotificationRepository = repositoryFactory.GetRepository(Enum.TryParse<StorageType>(this.configuration?[Constants.StorageType], out this.repo) ? this.repo : throw new Exception());
        }

        /// <summary>
        /// Trigger method invoked when a notification item is added to the queue.
        /// </summary>
        /// <param name="message">Serialized queue item.</param>
        [FunctionName("GDPRNotificationMappingProcessor")]
        public void Run([QueueTrigger("%GdprNotifEmailMapQueueName%", Connection = "AzureWebJobsStorage")]string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            this.logger.TraceInformation($"Started processing notificaiton to create emailid-notificationid mapping.");
            NotificationMappingQueueItem item = JsonConvert.DeserializeObject<NotificationMappingQueueItem>(message);
            NotificationType notifType = (NotificationType)Enum.Parse(typeof(NotificationType), item.NotificationType);
            var payloadJson = item.Payload?.ToString();

            if (string.IsNullOrEmpty(payloadJson))
            {
                this.logger.TraceError($"Notification Payload for type {item.NotificationType} can't be null or empty");
                return;
            }

            this.logger.TraceInformation($"processing notification mapping for notification type {item.NotificationType}");
            if (notifType == NotificationType.Mail)
            {
                var entities = JsonConvert.DeserializeObject<List<EmailNotificationQueueItem>>(payloadJson);
                this.emailNotificationRepository.CreateEmailIdNotificationForEmailsMapping(entities, item.ApplicationName);
            }
            else
            {
                var entities = JsonConvert.DeserializeObject<List<MeetingNotificationQueueItem>>(payloadJson);
                this.emailNotificationRepository.CreateEmailIdNotificationForMeetingInvitesMapping(entities, item.ApplicationName);
            }

            this.logger.TraceInformation($"Finished processing notificaiton for type {item.NotificationType} to create emailid-notificationid mapping.");
        }
    }
}
