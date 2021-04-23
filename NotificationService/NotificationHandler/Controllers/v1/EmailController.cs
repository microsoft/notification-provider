// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationHandler.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using NotificationHandler.Controllers.V1;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models.Request;
    using NotificationService.SvCommon.Attributes;

    /// <summary>
    /// Controller to handle email notifications.
    /// </summary>
    [Route("v1/email")]
    [Authorize(Policy = ApplicationConstants.AppNameAuthorizePolicy)]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class EmailController : BaseController
    {
        /// <summary>
        /// Instance of <see cref="IEmailHandlerManager"/>.
        /// </summary>
        private readonly IEmailHandlerManager emailHandlerManager;
        private readonly IMailTemplateManager templateManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailController"/> class.
        /// </summary>
        /// <param name="emailHandlerManager">An instance of <see cref="EmailManager"/>.</param>
        /// <param name="templateManager">An instance of <see cref="MailTemplateManager"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        /// <param name="correlationProvider">An instance of <see cref="ICorrelationProvider"/>.</param>
        public EmailController(IEmailHandlerManager emailHandlerManager, IMailTemplateManager templateManager, ILogger logger)
            : base(logger)
        {
            this.emailHandlerManager = emailHandlerManager ?? throw new System.ArgumentNullException(nameof(emailHandlerManager));
            this.templateManager = templateManager ?? throw new ArgumentNullException(nameof(templateManager));
        }

        /// <summary>
        /// Resend email notification items.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="notificationIds">Array of email notification ids.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("resend/{applicationName}")]
        public async Task<IActionResult> ResendEmailNotifications(string applicationName, [FromBody] string[] notificationIds)
        {
            var traceProps = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                this.logger.WriteException(new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName)));
                throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
            }

            traceProps[AIConstants.Application] = applicationName;
            if (notificationIds is null)
            {
                this.logger.WriteException(new System.ArgumentNullException(nameof(notificationIds)), traceProps);
                throw new System.ArgumentNullException(nameof(notificationIds));
            }

            if (notificationIds.Length == 0)
            {
                this.logger.WriteException(new System.ArgumentException("Notifications Ids list should not be empty", nameof(notificationIds)), traceProps);
                throw new System.ArgumentException("Notifications Ids list should not be empty", nameof(notificationIds));
            }

            traceProps[AIConstants.NotificationIds] = string.Join(',', notificationIds);

            IList<NotificationResponse> notificationResponses;
            this.logger.TraceInformation($"Started {nameof(this.ResendEmailNotifications)} method of {nameof(EmailController)}.", traceProps);
            notificationResponses = await this.emailHandlerManager.ResendNotifications(applicationName, notificationIds).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.ResendEmailNotifications)} method of {nameof(EmailController)}.", traceProps);
            return this.Accepted(notificationResponses);
        }

        /// <summary>
        /// Resend email notification items by Date Range.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="dateRange">Date Range to resubmit the notifications.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("resend/{applicationName}/bydaterange")]
        public async Task<IActionResult> ResendEmailNotificationsByDateRange(string applicationName, [FromBody] DateTimeRange dateRange)
        {
            var traceProps = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
            }

            if (dateRange is null)
            {
                throw new ArgumentNullException(nameof(dateRange));
            }

            traceProps[AIConstants.Application] = applicationName;
            if (dateRange is null)
            {
                this.LogAndThrowArgumentNullException("DateTimeRange can't be null.", nameof(dateRange), traceProps);
            }

            if ((dateRange.EndDate - dateRange.StartDate).TotalMinutes <= 0)
            {
                this.LogAndThrowArgumentNullException("StartDate value must be less than EndDate value.", nameof(dateRange), traceProps);
            }

            traceProps[ApplicationConstants.ResendDateRange] = JsonConvert.SerializeObject(dateRange);
            IList<NotificationResponse> notificationResponses;
            this.logger.TraceInformation($"Started {nameof(this.ResendEmailNotifications)} method of {nameof(EmailController)}.", traceProps);
            notificationResponses = await this.emailHandlerManager.ResendEmailNotificationsByDateRange(applicationName, dateRange).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.ResendEmailNotifications)} method of {nameof(EmailController)}.", traceProps);
            return this.Accepted(notificationResponses);
        }

        /// <summary>
        /// Queue email notification items for asynchronous processing.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="emailNotificationItems">Array of email notification items.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("queue/{applicationName}")]
        public async Task<IActionResult> QueueEmailNotifications(string applicationName, [FromBody] EmailNotificationItem[] emailNotificationItems)
        {
            var traceProps = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
            }

            if (emailNotificationItems is null)
            {
                throw new ArgumentNullException(nameof(emailNotificationItems));
            }

            if (emailNotificationItems.Length == 0)
            {
                this.logger.WriteException(new System.ArgumentException("Email Notification Items list should not be empty", nameof(emailNotificationItems)), traceProps);
                throw new System.ArgumentException("Email Notification Items list should not be empty", nameof(emailNotificationItems));
            }

            traceProps[AIConstants.Application] = applicationName;
            IList<NotificationResponse> notificationResponses;
            this.logger.TraceInformation($"Started {nameof(this.QueueEmailNotifications)} method of {nameof(EmailController)}.", traceProps);
            notificationResponses = await this.emailHandlerManager.QueueEmailNotifications(applicationName, emailNotificationItems).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.QueueEmailNotifications)} method of {nameof(EmailController)}.", traceProps);
            return this.Accepted(notificationResponses);
        }

        /// <summary>
        /// Saves mail template.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="mailTemplate"><see cref="MailTemplate"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("mailTemplate/{applicationName}")]
        public async Task<IActionResult> SaveMailTemplate(string applicationName, [FromBody] MailTemplate mailTemplate)
        {
            try
            {
                var traceProps = new Dictionary<string, string>();

                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    this.LogAndThrowArgumentNullException("Application Name cannot be null or empty.", nameof(applicationName), traceProps);
                }

                if (mailTemplate is null)
                {
                    this.LogAndThrowArgumentNullException("Mail template param should not be null", nameof(mailTemplate), traceProps);
                }

                if (string.IsNullOrWhiteSpace(mailTemplate?.TemplateId))
                {
                    this.LogAndThrowArgumentNullException("Template name should not be empty", nameof(mailTemplate), traceProps);
                }

                if (string.IsNullOrWhiteSpace(mailTemplate.Description))
                {
                    this.LogAndThrowArgumentNullException("Template description should not be empty", nameof(mailTemplate), traceProps);
                }

                if (string.IsNullOrWhiteSpace(mailTemplate.Content))
                {
                    this.LogAndThrowArgumentNullException("Template content should not be empty", nameof(mailTemplate), traceProps);
                }

                if (string.IsNullOrWhiteSpace(mailTemplate.TemplateType))
                {
                    this.LogAndThrowArgumentNullException("Template type should not be empty", nameof(mailTemplate), traceProps);
                }

                if (!(mailTemplate.TemplateType.ToLowerInvariant() == "xslt" || mailTemplate.TemplateType.ToLowerInvariant() == "text"))
                {
                    this.LogAndThrowArgumentNullException("Template type should be 'Text' or 'XSLT'", nameof(mailTemplate), traceProps);
                }

                traceProps[AIConstants.Application] = applicationName;
                bool result;
                this.logger.TraceInformation($"Started {nameof(this.SaveMailTemplate)} method of {nameof(EmailController)}.", traceProps);
                result = await this.templateManager.SaveEmailTemplate(applicationName, mailTemplate).ConfigureAwait(false);
                this.logger.TraceInformation($"Finished {nameof(this.SaveMailTemplate)} method of {nameof(EmailController)}.", traceProps);
                return this.Accepted(result);
            }
            catch (ArgumentNullException agNullEx)
            {
                return this.BadRequest(agNullEx.Message);
            }
            catch (ArgumentException agEx)
            {
                return this.BadRequest(agEx.Message);
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the mail template.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="mailTemplateName">Template name.</param>
        /// <returns><see cref="MailTemplate"/>.</returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("mailTemplate/{applicationName}/{mailTemplateName}")]
        public async Task<IActionResult> GetMailTemplate(string applicationName, string mailTemplateName)
        {
            try
            {
                var traceProps = new Dictionary<string, string>();

                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    this.LogAndThrowArgumentNullException("Application Name cannot be null or empty.", nameof(applicationName), traceProps);
                }

                if (string.IsNullOrWhiteSpace(mailTemplateName))
                {
                    this.LogAndThrowArgumentNullException("Template name should not be empty", nameof(mailTemplateName), traceProps);
                }

                traceProps[AIConstants.Application] = applicationName;
                MailTemplate mailTemplate;
                this.logger.TraceInformation($"Started {nameof(this.GetMailTemplate)} method of {nameof(EmailController)}.", traceProps);
                mailTemplate = await this.templateManager.GetMailTemplate(applicationName, mailTemplateName).ConfigureAwait(false);
                this.logger.TraceInformation($"Finished {nameof(this.GetMailTemplate)} method of {nameof(EmailController)}.", traceProps);
                return this.Accepted(mailTemplate);
            }
            catch (ArgumentNullException agNullEx)
            {
                return this.BadRequest(agNullEx.Message);
            }
            catch (ArgumentException agEx)
            {
                return this.BadRequest(agEx.Message);
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes the mail template.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="mailTemplateName">Template name.</param>
        /// <returns>status of delete operation.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("deleteTemplate/{applicationName}/{mailTemplateName}")]
        public async Task<IActionResult> DeleteMailTemplate(string applicationName, string mailTemplateName)
        {
            try
            {
                var traceProps = new Dictionary<string, string>();

                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    this.LogAndThrowArgumentNullException("Application Name cannot be null or empty.", nameof(applicationName), traceProps);
                }

                if (string.IsNullOrWhiteSpace(mailTemplateName))
                {
                    this.LogAndThrowArgumentNullException("Template name should not be empty", nameof(mailTemplateName), traceProps);
                }

                traceProps[AIConstants.Application] = applicationName;
                this.logger.TraceInformation($"Started {nameof(this.DeleteMailTemplate)} method of {nameof(EmailController)}.", traceProps);
                var status = await this.templateManager.DeleteMailTemplate(applicationName, mailTemplateName).ConfigureAwait(false);
                this.logger.TraceInformation($"Finished {nameof(this.DeleteMailTemplate)} method of {nameof(EmailController)}.", traceProps);
                return this.Accepted(status);
            }
            catch (ArgumentNullException agNullEx)
            {
                return this.BadRequest(agNullEx.Message);
            }
            catch (ArgumentException agEx)
            {
                return this.BadRequest(agEx.Message);
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
                throw;
            }
        }
    }
}