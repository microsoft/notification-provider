// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;
    using NotificationService.Contracts.Extensions;
    using NotificationService.Contracts.Models;
    using NotificationService.Contracts.Models.Request;
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
        /// <param name="templateManager">Instance of templateManager.</param>
        /// <param name="templateMerge">Instance of templateMerge.</param>
        public EmailManager(
            IConfiguration configuration,
            IRepositoryFactory repositoryFactory,
            ILogger logger,
            IMailTemplateManager templateManager,
            ITemplateMerge templateMerge)
        {
            this.repositoryFactory = repositoryFactory;
            this.configuration = configuration;
            this.emailNotificationRepository = repositoryFactory.GetRepository(Enum.TryParse<StorageType>(this.configuration?[ConfigConstants.StorageType], out this.repo) ? this.repo : throw new Exception());
            this.logger = logger;
            this.templateManager = templateManager;
            this.templateMerge = templateMerge;
        }

        /// <summary>
        /// Constructs list of responses for each notification item entity.
        /// </summary>
        /// <param name="notificationResponses">List of notification response items.</param>
        /// <param name="notificationItemEntities">List of notification item entities.</param>
        /// <returns>Notification response items list with response data populated.</returns>
        public IList<NotificationResponse> NotificationEntitiesToResponse(IList<NotificationResponse> notificationResponses, IList<MeetingNotificationItemEntity> notificationItemEntities)
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
        /// Creates the notification entity records in database with the input status.
        /// </summary>
        /// <param name="applicationName">Application associated with notification items.</param>
        /// <param name="emailNotificationItems">Array of notifications items to be created.</param>
        /// <param name="status">Status of the items to be created.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<IList<EmailNotificationItemEntity>> CreateNotificationEntities(string applicationName, EmailNotificationItem[] emailNotificationItems, NotificationItemStatus status)
        {
            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.EmailNotificationCount] = emailNotificationItems?.Length.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.CreateNotificationEntities)} method of {nameof(EmailManager)}.", traceProps);
            IList<EmailNotificationItemEntity> notificationEntities = new List<EmailNotificationItemEntity>();

            foreach (var item in emailNotificationItems)
            {
                var notificationEntity = item.ToEntity(applicationName);
                notificationEntity.NotificationId = !string.IsNullOrWhiteSpace(item.NotificationId) ? item.NotificationId : Guid.NewGuid().ToString();
                notificationEntity.Id = Guid.NewGuid().ToString();
                notificationEntity.CreatedDateTime = DateTime.UtcNow;
                notificationEntity.UpdatedDateTime = DateTime.UtcNow;
                notificationEntity.Status = status;
                notificationEntities.Add(notificationEntity);
            }

            await this.emailNotificationRepository.CreateEmailNotificationItemEntities(notificationEntities, applicationName).ConfigureAwait(false);
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

                if (string.IsNullOrEmpty(notification.Body) && !string.IsNullOrEmpty(notification.TemplateId))
                {
                    MailTemplate template = await this.templateManager.GetMailTemplate(applicationName, notification.TemplateId).ConfigureAwait(false);
                    if (template == null)
                    {
                        throw new ArgumentException("Template cannot be found, please provide a valid template and application name");
                    }

                    notificationBody = this.templateMerge.CreateMailBodyUsingTemplate(template.TemplateType, template.Content, notification.TemplateData);
                }
                else
                {
                    if (!string.IsNullOrEmpty(notification.Body))
                    {
                        notificationBody = notification.Body;
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

        /// <summary>
        /// Gets the notification message body asynchronous.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="notification">The notification.</param>
        /// <returns>A <see cref="Task{TResult}" /> representing the result of the asynchronous operation.</returns>
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
        public async Task<MessageBody> GetMeetingInviteBodyAsync(string applicationName, MeetingNotificationItemEntity notification)
        {
            this.logger.TraceInformation($"Started {nameof(this.GetMeetingInviteBodyAsync)} method of {nameof(EmailManager)}.");
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

                if (string.IsNullOrEmpty(notification.Body) && !string.IsNullOrEmpty(notification.TemplateId))
                {
                    MailTemplate template = await this.templateManager.GetMailTemplate(applicationName, notification.TemplateId).ConfigureAwait(false);
                    if (template == null)
                    {
                        throw new ArgumentException("Template cannot be found, please provide a valid template and application name");
                    }

                    notificationBody = this.templateMerge.CreateMailBodyUsingTemplate(template.TemplateType, template.Content, notification.TemplateData);
                }
                else
                {
                    if (!string.IsNullOrEmpty(notification.Body))
                    {
                        notificationBody = notification.Body;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
                throw;
            }

            MessageBody messageBody = new MessageBody { Content = notificationBody, ContentType = Common.ApplicationConstants.EmailBodyContentType };
            this.logger.TraceInformation($"Finished {nameof(this.GetMeetingInviteBodyAsync)} method of {nameof(EmailManager)}.");
            return messageBody;
        }

        /// <inheritdoc/>
        public async Task<IList<MeetingNotificationItemEntity>> CreateMeetingNotificationEntities(string applicationName, MeetingNotificationItem[] meetingNotificationItems, NotificationItemStatus status)
        {
            if (meetingNotificationItems is null)
            {
                throw new ArgumentNullException(nameof(meetingNotificationItems));
            }

            var traceProps = new Dictionary<string, string>();
            traceProps[AIConstants.Application] = applicationName;
            traceProps[AIConstants.MeetingNotificationCount] = meetingNotificationItems?.Length.ToString(CultureInfo.InvariantCulture);

            this.logger.TraceInformation($"Started {nameof(this.CreateNotificationEntities)} method of {nameof(EmailManager)}.", traceProps);
            IList<MeetingNotificationItemEntity> notificationEntities = new List<MeetingNotificationItemEntity>();

            foreach (var item in meetingNotificationItems)
            {
                var notificationEntity = item.ToEntity(applicationName);
                notificationEntity.NotificationId = !string.IsNullOrWhiteSpace(item.NotificationId) ? item.NotificationId : Guid.NewGuid().ToString();
                notificationEntity.Id = Guid.NewGuid().ToString();
                notificationEntity.CreatedDateTime = DateTime.UtcNow;
                notificationEntity.UpdatedDateTime = DateTime.UtcNow;
                notificationEntity.Status = status;
                notificationEntities.Add(notificationEntity);
            }

            await this.emailNotificationRepository.CreateMeetingNotificationItemEntities(notificationEntities, applicationName).ConfigureAwait(false);
            this.logger.TraceInformation($"Completed {nameof(this.CreateNotificationEntities)} method of {nameof(EmailManager)}.", traceProps);
            return notificationEntities;
        }

        /// <inheritdoc/>
        public async Task<IList<EmailNotificationItemEntity>> GetEmailNotificationsByDateRangeAndStatus(string applicationName, DateTimeRange dateRange, List<NotificationItemStatus> statusList)
        {
            return await this.emailNotificationRepository.GetPendingOrFailedEmailNotificationsByDateRange(dateRange, applicationName, statusList).ConfigureAwait(false);
        }
    }
}
