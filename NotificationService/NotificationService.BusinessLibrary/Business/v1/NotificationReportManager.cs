// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Extensions;
    using NotificationService.Contracts.Models.Reports;
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
        /// Instance of <see cref="IMailTemplateRepository"/>.
        /// </summary>
        private readonly IMailTemplateRepository mailTemplateRepository;

        /// <summary>
        /// Instance of <see cref="IMailTemplateManager"/>.
        /// </summary>
        private readonly IMailTemplateManager templateManager;

        /// <summary>
        /// Instance of <see cref="ITemplateMerge"/>.
        /// </summary>
        private readonly ITemplateMerge templateMerge;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationReportManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mailTemplateRepository">The mail template repository.</param>
        /// <param name="templateManager">The template manager.</param>
        /// <param name="templateMerge">The template merge.</param>
        /// <exception cref="Exception">Unknown Database Type.</exception>
        /// <exception cref="System.ArgumentNullException">mailTemplateRepository.</exception>
        public NotificationReportManager(
            ILogger logger,
            IRepositoryFactory repositoryFactory,
            IConfiguration configuration,
            IMailTemplateRepository mailTemplateRepository,
            IMailTemplateManager templateManager,
            ITemplateMerge templateMerge)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.emailNotificationRepository = repositoryFactory?.GetRepository(Enum.TryParse<StorageType>(this.configuration?[ConfigConstants.StorageType], out this.repo) ? this.repo : throw new Exception("Unknown Database Type"));
            this.templateManager = templateManager;
            this.templateMerge = templateMerge;
            this.mailTemplateRepository = mailTemplateRepository ?? throw new System.ArgumentNullException(nameof(mailTemplateRepository));
        }

        /// <inheritdoc/>
        public async Task<Tuple<IList<NotificationReportResponse>, TableContinuationToken>> GetReportNotifications(NotificationReportRequest notificationReportRequest)
        {
            // Map the request object to filters
            if (notificationReportRequest == null)
            {
                throw new ArgumentNullException($"{nameof(notificationReportRequest)}");
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
            var traceprops = new Dictionary<string, string>();
            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.NotificationIds] = notificationId;
            this.logger.TraceInformation($"Started {nameof(this.GetNotificationMessage)} method in {nameof(NotificationReportManager)}.", traceprops);
            try
            {
                EmailNotificationItemEntity notification = await this.emailNotificationRepository.GetEmailNotificationItemEntity(notificationId, applicationName).ConfigureAwait(false);

                if (notification != null)
                {
                    MessageBody body = await this.GetNotificationMessageBodyAsync(applicationName, notification).ConfigureAwait(false);
                    this.logger.TraceInformation($"Completed {nameof(this.GetNotificationMessage)} method in {nameof(NotificationReportManager)}.", traceprops);

                    return notification.ToGraphEmailMessage(body);
                }
                else
                {
                    throw new ArgumentNullException(notificationId, "No Email entity found for the input notification id: ");
                }
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Tuple<IList<MeetingInviteReportResponse>, TableContinuationToken>> GetMeetingInviteReportNotifications(NotificationReportRequest notificationReportRequest)
        {
            // Map the request object to filters
            if (notificationReportRequest == null)
            {
                throw new ArgumentNullException($"{nameof(notificationReportRequest)}");
            }

            var meetingNotificationItemEntities = await this.emailNotificationRepository.GetMeetingInviteNotifications(notificationReportRequest).ConfigureAwait(false);
            TableContinuationToken token = meetingNotificationItemEntities.Item2;
            List<MeetingInviteReportResponse> responseList = new List<MeetingInviteReportResponse>();
            foreach (var item in meetingNotificationItemEntities.Item1)
            {
                responseList.Add(MeetingNotificationItemEntityExtensions.ToMeetingInviteReportResponse(item));
            }

            Tuple<IList<MeetingInviteReportResponse>, TableContinuationToken> tuple = new Tuple<IList<MeetingInviteReportResponse>, TableContinuationToken>(responseList, token);
            return tuple;
        }

        /// <inheritdoc/>
        public async Task<IList<MailTemplateInfo>> GetAllTemplateEntities(string applicationName)
        {
            try
            {
                this.logger.TraceInformation($"Started {nameof(this.GetAllTemplateEntities)} method in {nameof(NotificationReportManager)}.");
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

        /// <inheritdoc/>
        public IList<string> GetApplications()
        {
            this.logger.TraceInformation($"Started {nameof(this.GetApplications)} method of {nameof(NotificationReportManager)}.");
            var applicationAccounts = JsonConvert.DeserializeObject<List<ApplicationAccounts>>(this.configuration?[ConfigConstants.ApplicationAccountsConfigSectionKey]);
            var applications = applicationAccounts.Select(appName => appName.ApplicationName).ToList();
            this.logger.TraceInformation($"Finished {nameof(this.GetApplications)} method of {nameof(NotificationReportManager)}.");
            return applications;
        }

        /// <inheritdoc/>
        public async Task<MeetingInviteMessage> GetMeetingNotificationMessage(string applicationName, string notificationId)
        {
            var traceprops = new Dictionary<string, string>();
            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.NotificationIds] = notificationId;
            this.logger.TraceInformation($"Started {nameof(this.GetNotificationMessage)} method in {nameof(NotificationReportManager)}.", traceprops);
            try
            {
                MeetingNotificationItemEntity notification = await this.emailNotificationRepository.GetMeetingNotificationItemEntity(notificationId, applicationName).ConfigureAwait(false);

                if (notification != null)
                {
                    MessageBody body = await this.GetNotificationMessageBodyAsync(applicationName, notification).ConfigureAwait(false);
                    this.logger.TraceInformation($"Completed {nameof(this.GetNotificationMessage)} method in {nameof(NotificationReportManager)}.", traceprops);
                    return notification.ToMeetingInviteReportMessage(body);
                }
                else
                {
                    throw new ArgumentNullException(notificationId, "No MeetingInvite entity found for the input notification id: ");
                }
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
                throw;
            }
        }

        /// <summary>
        /// Get Notification Message Body Async.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="notification">email notification item entity.</param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> representing the result of the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// applicationName - applicationName cannot be null or empty.
        /// or
        /// notification - notification cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// TemplateData cannot be null or empty.
        /// or
        /// Template cannot be found, please provide a valid template and application name.
        /// </exception>
        private async Task<MessageBody> GetNotificationMessageBodyAsync(string applicationName, NotificationItemBaseEntity notification)
        {
            string body = string.Empty;
            string templateId = string.Empty;
            string templateData = string.Empty;

            if (notification is EmailNotificationItemEntity)
            {
                var notifEntity = (EmailNotificationItemEntity)notification;
                body = notifEntity.Body;
                templateId = notifEntity.TemplateId;
                templateData = notifEntity.TemplateData;
            }
            else
            {
                var notifEntity = (MeetingNotificationItemEntity)notification;
                body = notifEntity.Body;
                templateId = notifEntity.TemplateId;
                templateData = notifEntity.TemplateData;
            }

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

                if (string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(templateId))
                {
                    MailTemplate template = await this.templateManager.GetMailTemplate(applicationName, templateId).ConfigureAwait(false);
                    if (template == null)
                    {
                        throw new ArgumentException("Template cannot be found, please provide a valid template and application name");
                    }

                    notificationBody = this.templateMerge.CreateMailBodyUsingTemplate(template.TemplateType, template.Content, templateData);
                }
                else
                {
                    if (!string.IsNullOrEmpty(body))
                    {
                        notificationBody = body;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
                throw;
            }

            MessageBody messageBody = new MessageBody { Content = notificationBody, ContentType = Common.ApplicationConstants.EmailBodyContentType };
            this.logger.TraceInformation($"Finished {nameof(this.GetNotificationMessageBodyAsync)} method of {nameof(EmailManager)}.");
            return messageBody;
        }
    }
}