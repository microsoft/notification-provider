// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationHandler.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Cosmos.Table;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.SvCommon.Attributes;

    /// <summary>
    /// Controller to handle notification client configs.
    /// </summary>
    [Route("v1/report")]
    [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class NotificationReportController : Controller
    {
        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of <see cref="INotificationReportManager"/>.
        /// </summary>
        private readonly INotificationReportManager notificationReportManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationReportController"/> class.
        /// </summary>
        /// <param name="notificationReportManager">An instance of <see cref="NotificationReportManager"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        public NotificationReportController(
            INotificationReportManager notificationReportManager,
            ILogger logger)
        {
            this.notificationReportManager = notificationReportManager ?? throw new System.ArgumentNullException(nameof(notificationReportManager));
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// API to get Applications.
        /// </summary>
        /// <returns>Returns list of applications.</returns>
        [HttpGet]
        [Route("applications")]
        public IActionResult GetApplications()
        {
            var result = this.notificationReportManager.GetApplications();
            return this.Accepted(result);
        }

        /// <summary>
        /// Returns all filtered Email Notification Entities for reporting.
        /// </summary>
        /// <param name="notificationFilterRequest">Request with filter parameters to get notifications for reporting.</param>
        /// <returns>A <see cref="EmailNotificationItemEntity"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Route("notifications")]
        public async Task<IActionResult> GetReportNotifications([FromBody] NotificationReportRequest notificationFilterRequest)
        {
            if (notificationFilterRequest is null)
            {
                throw new System.ArgumentException("Notification Filter Request cannot be null");
            }

            Tuple<IList<NotificationReportResponse>, TableContinuationToken> notificationResponses;
            this.logger.TraceInformation($"Started {nameof(this.GetReportNotifications)} method of {nameof(NotificationReportController)}.");
            notificationResponses = await this.notificationReportManager.GetReportNotifications(notificationFilterRequest).ConfigureAwait(false);
            string nextPartitionKey = notificationResponses.Item2?.NextPartitionKey;
            string nextRowKey = notificationResponses.Item2?.NextRowKey;
            if (nextPartitionKey != null && nextRowKey != null)
            {
                this.Response.Headers.Add("Access-Control-Expose-Headers", "X-NextPartitionKey, X-NextRowKey");
                this.Response.Headers.Add("X-NextPartitionKey", nextPartitionKey);
                this.Response.Headers.Add("X-NextRowKey", nextRowKey);
            }

            this.logger.TraceInformation($"Finished {nameof(this.GetReportNotifications)} method of {nameof(NotificationReportController)}.");
            return new OkObjectResult(notificationResponses.Item1);
        }

        /// <summary>
        /// Returns Email Message for reporting.
        /// </summary>
        /// <param name="applicationName">Application.</param>
        /// <param name="notificationId">notificationId of the email notification.</param>
        /// <returns>A <see cref="EmailMessage"/> returns the notification Message.</returns>
        [HttpGet]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("notificationMessage/{applicationName}/{notificationId}")]
        public async Task<IActionResult> GetNotificationMessage(string applicationName, string notificationId)
        {
            try
            {
                var traceProps = new Dictionary<string, string>();
                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    this.LogAndThrowArgumentNullException("Application Name cannot be null or empty.", nameof(applicationName), traceProps);
                }

                traceProps[AIConstants.Application] = applicationName;
                if (string.IsNullOrWhiteSpace(notificationId))
                {
                    this.LogAndThrowArgumentNullException("notificationId should not be empty", nameof(notificationId), traceProps);
                }

                this.logger.TraceInformation($"Started {nameof(this.GetNotificationMessage)} method of {nameof(NotificationReportController)}.");
                EmailMessage emailMessage = await this.notificationReportManager.GetNotificationMessage(applicationName, notificationId).ConfigureAwait(false);

                this.logger.TraceInformation($"Finished {nameof(this.GetNotificationMessage)} method of {nameof(NotificationReportController)}.");
                return new OkObjectResult(emailMessage);
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
        /// Returns all filtered meeting invite Notification Entities for reporting.
        /// </summary>
        /// <param name="notificationFilterRequest">Request with filter parameters to get meeting invite notifications for reporting.</param>
        /// <returns> list of MeetingInviteReportResponse.</returns>
        [HttpPost]
        [Route("meetingInvites")]
        public async Task<IActionResult> GetMeetingInviteReportNotifications([FromBody] NotificationReportRequest notificationFilterRequest)
        {
            if (notificationFilterRequest is null)
            {
                throw new System.ArgumentException("Notification Filter Request cannot be null");
            }

            Tuple<IList<MeetingInviteReportResponse>, TableContinuationToken> notificationResponses;
            this.logger.TraceInformation($"Started {nameof(this.GetMeetingInviteReportNotifications)} method of {nameof(NotificationReportController)}.");
            notificationResponses = await this.notificationReportManager.GetMeetingInviteReportNotifications(notificationFilterRequest).ConfigureAwait(false);
            string nextPartitionKey = notificationResponses.Item2?.NextPartitionKey;
            string nextRowKey = notificationResponses.Item2?.NextRowKey;
            if (nextPartitionKey != null && nextRowKey != null)
            {
                this.Response.Headers.Add("Access-Control-Expose-Headers", "X-NextPartitionKey, X-NextRowKey");
                this.Response.Headers.Add("X-NextPartitionKey", nextPartitionKey);
                this.Response.Headers.Add("X-NextRowKey", nextRowKey);
            }

            this.logger.TraceInformation($"Finished {nameof(this.GetMeetingInviteReportNotifications)} method of {nameof(NotificationReportController)}.");
            return new OkObjectResult(notificationResponses.Item1);
        }

        /// <summary>
        /// Gets All Template Entities for the input application.
        /// </summary>
        /// <param name="applicationName">Application Name.</param>
        /// <returns>a list of template entities <see cref="MailTemplate"/>.</returns>
        [HttpGet]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("templates/{applicationName}")]
        public async Task<IActionResult> GetAllTemplateEntities(string applicationName)
        {
            try
            {
                var traceProps = new Dictionary<string, string>();
                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    this.LogAndThrowArgumentNullException("Application Name cannot be null or empty.", nameof(applicationName), traceProps);
                }

                traceProps[AIConstants.Application] = applicationName;

                this.logger.TraceInformation($"Started {nameof(this.GetAllTemplateEntities)} method of {nameof(NotificationReportController)}.");
                IList<MailTemplateInfo> mailTemplatesInfo = await this.notificationReportManager.GetAllTemplateEntities(applicationName).ConfigureAwait(false);
                this.logger.TraceInformation($"Finished {nameof(this.GetAllTemplateEntities)} method of {nameof(NotificationReportController)}.");
                return new OkObjectResult(mailTemplatesInfo);
            }
            catch (ArgumentNullException agNullEx)
            {
                return this.BadRequest(agNullEx.Message);
            }
            catch (Exception ex)
            {
                this.logger.WriteException(ex);
                throw;
            }
        }

        /// <summary>
        /// Logs and rethrow the exception.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="inputName">Name of input type.</param>
        /// <param name="traceProps">custom properties, add more dimensions to this, so it will be easy to trace and query.</param>
        private void LogAndThrowArgumentNullException(string message, string inputName, Dictionary<string, string> traceProps)
        {
            var argumentException = new System.ArgumentNullException(inputName, message);
            this.logger.TraceInformation(argumentException.Message, traceProps);
            throw argumentException;
        }
    }
}
