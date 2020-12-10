// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Azure.Storage.Shared.Protocol;
    using Microsoft.Extensions.Configuration;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Logger;
    using NotificationService.Common.Utility;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Extensions;
    using NotificationService.Data;
    using NotificationService.Data.Interfaces;

    /// <summary>
    /// Handles the Reporting Requests for Notifications.
    /// </summary>
    public class NotificationReportManager : INotificationReportManager
    {
        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of <see cref="IEmailNotificationRepository"/>.
        /// </summary>
        private readonly IEmailNotificationRepository emailNotificationRepository;

        /// <summary>
        /// Enum to specify type of database.
        /// </summary>
        private readonly StorageType repo;

        /// <summary>
        /// Instance of Application Configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Instance of <see cref="IEmailManager"/>.
        /// </summary>
        private readonly IEmailManager emailManager;

        /// <summary>
        /// Instance of <see cref="IMailTemplateRepository"/>.
        /// </summary>
        private readonly IMailTemplateRepository mailTemplateRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationReportManager"/> class.
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        /// <param name="repositoryFactory">An instance of <see cref="IRepositoryFactory"/>.</param>
        /// <param name="configuration">An instance of <see cref="IConfiguration"/>.</param>
        /// <param name="emailManager">An instance of <see cref="EmailManager"/>.</param>
        /// <param name="mailTemplateRepository">An instance of <see cref="IMailTemplateRepository"/>.</param>
        public NotificationReportManager(
            ILogger logger,
            IRepositoryFactory repositoryFactory,
            IConfiguration configuration,
            IEmailManager emailManager,
            IMailTemplateRepository mailTemplateRepository)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.emailNotificationRepository = this.emailNotificationRepository = repositoryFactory.GetRepository(Enum.TryParse<StorageType>(this.configuration?[NotificationService.Common.Constants.StorageType], out repo) ? repo : throw new Exception("Unknown Database Type"));
            this.emailManager = emailManager;
            this.mailTemplateRepository = mailTemplateRepository ?? throw new System.ArgumentNullException(nameof(mailTemplateRepository));
        }

        /// <inheritdoc/>
        public async Task<Tuple<IList<NotificationReportResponse>, TableContinuationToken>> GetReportNotifications(NotificationReportRequest notificationReportRequest)
        {
            // Map the request object to filters
            if (notificationReportRequest == null)
            {
                throw new ArgumentNullException($"The Notification Report Request cannot be null");
            }

            var emailNotificationItemEntities = await this.emailNotificationRepository.GetEmailNotifications(notificationReportRequest).ConfigureAwait(false);
            TableContinuationToken token = emailNotificationItemEntities.Item2;
            List<NotificationReportResponse> responseList = new List<NotificationReportResponse>();
            foreach (var item in emailNotificationItemEntities.Item1)
            {
                responseList.Add(EmailNotificationItemEntityExtensions.ToNotificationReportResponse(item));
            }

            Tuple<IList<NotificationReportResponse>, TableContinuationToken> tuple = new Tuple<IList<NotificationReportResponse>, TableContinuationToken>(responseList, token);
            return tuple;
        }

        /// <inheritdoc/>
        public async Task<EmailMessage> GetNotificationMessage(string applicationName, string notificationId)
        {
            this.logger.TraceVerbose($"Started {nameof(this.GetNotificationMessage)} method in {nameof(NotificationReportManager)}.");
            try
            {
                this.logger.TraceVerbose($"Started {nameof(this.emailNotificationRepository.GetEmailNotificationItemEntity)} method in {nameof(NotificationReportManager)}.");
                EmailNotificationItemEntity notification = await this.emailNotificationRepository.GetEmailNotificationItemEntity(notificationId).ConfigureAwait(false);
                this.logger.TraceVerbose($"Completed {nameof(this.emailNotificationRepository.GetEmailNotificationItemEntity)} method in {nameof(NotificationReportManager)}.");

                if (notification != null)
                {
                    MessageBody body = await this.emailManager.GetNotificationMessageBodyAsync(applicationName, notification).ConfigureAwait(false);
                    this.logger.TraceVerbose($"Completed {nameof(this.GetNotificationMessage)} method in {nameof(NotificationReportManager)}.");

                    return notification.ToGraphEmailMessage(body);
                }
                else
                {
                    throw new ArgumentNullException("Null entity found for the input notification id: ", notificationId);
                }
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IList<MailTemplateInfo>> GetAllTemplateEntities(string applicationName)
        {
            try
            {
                this.logger.TraceVerbose($"Started {nameof(this.GetAllTemplateEntities)} method in {nameof(NotificationReportManager)}.");
                IList<MailTemplateEntity> mailTemplateEntities = await this.mailTemplateRepository.GetAllTemplateEntities(applicationName).ConfigureAwait(false);
                IList<MailTemplateInfo> mailTemplatesInfo = new List<MailTemplateInfo>();
                foreach (var item in mailTemplateEntities)
                    {
                    mailTemplatesInfo.Add(item.ToTemplateInfoContract());
                }

                this.logger.TraceVerbose($"Completed {nameof(this.GetAllTemplateEntities)} method in {nameof(NotificationReportManager)}.");
                return mailTemplatesInfo;
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
                throw;
            }
        }
    }
}