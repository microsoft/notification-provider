﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Encryption;
    using NotificationService.Common.Logger;
    using NotificationService.Common.Utility;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Extensions;
    using NotificationService.Data;
    using NotificationService.Data.Interfaces;

    /// <summary>
    /// Business Manager Common functions.
    /// </summary>
    public class EmailManager : IEmailManager
    {
        /// <summary>
        /// Instance of <see cref="IRepositoryFactory"/>.
        /// </summary>
        private readonly IRepositoryFactory repositoryFactory;

        /// <summary>
        /// Instance of Application Configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Instance of <see cref="IEmailNotificationRepository"/>.
        /// </summary>
        private readonly IEmailNotificationRepository emailNotificationRepository;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of <see cref="IEncryptionService"/>.
        /// </summary>
        private readonly IEncryptionService encryptionService;

        /// <summary>
        /// Enum to specify type of database.
        /// </summary>
        private readonly StorageType repo;

        /// <summary>
        /// Instance of <see cref="IMailTemplateManager"/>.
        /// </summary>
        private readonly IMailTemplateManager templateManager;

        /// <summary>
        /// Instance of <see cref="ITemplateMerge"/>.
        /// </summary>
        private readonly ITemplateMerge templateMerge;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailManager"/> class.
        /// </summary>
        /// <param name="configuration">An instance of <see cref="IConfiguration"/>.</param>
        /// <param name="repositoryFactory">An instance of <see cref="IRepositoryFactory"/>.</param>
        /// <param name="logger">Instance of Logger.</param>
        /// <param name="encryptionService">Instance of Encryption Service.</param>
        /// <param name="templateManager">Instance of templateManager.</param>
        /// <param name="templateMerge">Instance of templateMerge.</param>
        public EmailManager(
            IConfiguration configuration,
            IRepositoryFactory repositoryFactory,
            ILogger logger,
            IEncryptionService encryptionService,
            IMailTemplateManager templateManager,
            ITemplateMerge templateMerge)
        {
            this.repositoryFactory = repositoryFactory;
            this.configuration = configuration;
            this.emailNotificationRepository = repositoryFactory.GetRepository(Enum.TryParse<StorageType>(this.configuration?[Constants.StorageType], out repo) ? repo : throw new Exception());
            this.logger = logger;
            this.encryptionService = encryptionService;
            this.templateManager = templateManager;
            this.templateMerge = templateMerge;
        }

        /// <summary>
        /// Creates the notification entity records in database with the input status.
        /// </summary>
        /// <param name="applicationName">Application associated with notification items.</param>
        /// <param name="emailNotificationItems">Array of notifications items to be created.</param>
        /// <param name="status">Status of the items to be created.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<IList<EmailNotificationItemEntity>> CreateNotificationEntities(string applicationName, EmailNotificationItem[] emailNotificationItems, NotificationItemStatus status)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[Constants.Application] = applicationName;

            this.logger.TraceInformation($"Started {nameof(this.CreateNotificationEntities)} method of {nameof(EmailManager)}.", traceProps);
            IList<EmailNotificationItemEntity> notificationEntities = new List<EmailNotificationItemEntity>();

            foreach (var item in emailNotificationItems)
            {
                var notificationEntity = item.ToEntity(applicationName, this.encryptionService);
                notificationEntity.NotificationId = !string.IsNullOrWhiteSpace(item.NotificationId) ? item.NotificationId : Guid.NewGuid().ToString();
                notificationEntity.Id = Guid.NewGuid().ToString();
                notificationEntity.CreatedDateTime = DateTime.UtcNow;
                notificationEntity.UpdatedDateTime = DateTime.UtcNow;
                notificationEntity.Status = status;
                notificationEntities.Add(notificationEntity);
            }

            await this.emailNotificationRepository.CreateEmailNotificationItemEntities(notificationEntities).ConfigureAwait(false);
            this.logger.TraceInformation($"Completed {nameof(this.CreateNotificationEntities)} method of {nameof(EmailManager)}.", traceProps);
            return notificationEntities;
        }

        /// <summary>
        /// Constructs list of responses for each notification item entity.
        /// </summary>
        /// <param name="notificationResponses">List of notification response items.</param>
        /// <param name="notificationItemEntities">List of notification item entities.</param>
        /// <returns>Notification response items list with response data populated.</returns>
        public IList<NotificationResponse> NotificationEntitiesToResponse(IList<NotificationResponse> notificationResponses, IList<EmailNotificationItemEntity> notificationItemEntities)
        {
            notificationItemEntities.ToList().ForEach(nie => notificationResponses.Add(new NotificationResponse()
            {
                NotificationId = nie.NotificationId,
                Status = nie.Status,
                TrackingId = nie.TrackingId,
                ErrorMessage = nie.ErrorMessage,
            }));

            return notificationResponses;
        }

        /// <summary>
        /// Gets the Email Notification Message Body.
        /// </summary>
        /// <param name="applicationName">The application Name.</param>
        /// <param name="notification">The Notification Entity.</param>
        /// <returns>Returns the message body.</returns>
        public async Task<MessageBody> GetNotificationMessageBodyAsync(string applicationName, EmailNotificationItemEntity notification)
        {
            this.logger.TraceInformation($"Started {nameof(this.GetNotificationMessageBodyAsync)} method of {nameof(EmailManager)}.");
            string notificationBody = null;
            try
            {
                if (string.IsNullOrEmpty(applicationName))
                {
                    throw new ArgumentNullException(nameof(applicationName), "applicationName cannot be null or empty.");
                }

                if (notification is null)
                {
                    throw new ArgumentNullException(nameof(notification), "notification cannot be null.");
                }

                if (string.IsNullOrEmpty(notification.Body) && !string.IsNullOrEmpty(notification.TemplateName))
                {
                    if (string.IsNullOrEmpty(notification.TemplateData))
                    {
                        throw new ArgumentException("TemplateData cannot be null or empty.");
                    }

                    MailTemplate template = await this.templateManager.GetMailTemplate(applicationName, notification.TemplateName).ConfigureAwait(false);
                    if (template == null)
                    {
                        throw new ArgumentException("Template cannot be found, please provide a valid template and application name");
                    }

                    notificationBody = this.templateMerge.CreateMailBodyUsingTemplate(template.TemplateType, template.Content, this.encryptionService.Decrypt(notification.TemplateData));
                }
                else
                {
                    notificationBody = this.encryptionService.Decrypt(notification.Body);
                }
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
                throw;
            }

            MessageBody messageBody = new MessageBody { Content = notificationBody, ContentType = Common.Constants.EmailBodyContentType };
            this.logger.TraceInformation($"Finished {nameof(this.GetNotificationMessageBodyAsync)} method of {nameof(EmailManager)}.");
            return messageBody;
        }
    }
}
