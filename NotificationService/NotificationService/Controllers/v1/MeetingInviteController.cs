// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models;
    using NotificationService.SvCommon.Attributes;
    using NotificationService.SvCommon.Common;

    /// <summary>
    /// Controller to handle email notifications.
    /// </summary>
    [Route("v1/meetinginvite")]
    [Authorize(Policy = ApplicationConstants.AppNameAuthorizePolicy)]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class MeetingInviteController : Controller
    {
        /// <summary>
        /// Instance of <see cref="IEmailManager"/>.
        /// </summary>
        private readonly IEmailServiceManager emailServiceManager;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingInviteController"/> class.
        /// </summary>
        /// <param name="emailServiceManager">An instance of <see cref="IEmailServiceManager"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        public MeetingInviteController(IEmailServiceManager emailServiceManager, ILogger logger)
        {
            this.emailServiceManager = emailServiceManager ?? throw new System.ArgumentNullException(nameof(emailServiceManager));
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Process queued meeting notification items. Invoked from a trigger like Function App.
        /// Pre-requisite for this method is to ensure that the notification record already exists in queue and database.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="queueNotificationItem">Queue Notification item.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("process/{applicationName}")]
        public async Task<IList<NotificationResponse>> ProcessQueuedMeetingNotifications(string applicationName, [FromBody] QueueNotificationItem queueNotificationItem)
        {
            var traceprops = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new System.ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
            }

            if (queueNotificationItem is null)
            {
                throw new System.ArgumentNullException(nameof(queueNotificationItem));
            }

            if (queueNotificationItem.NotificationIds.Length == 0)
            {
                throw new ArgumentException("Notification IDs list should not be empty.", nameof(queueNotificationItem));
            }

            traceprops[AIConstants.Application] = applicationName;
            traceprops[AIConstants.NotificationIds] = string.Join(',', queueNotificationItem.NotificationIds);
            traceprops[AIConstants.MeetingNotificationCount] = queueNotificationItem.NotificationIds.Length.ToString(CultureInfo.InvariantCulture);

            IList<NotificationResponse> notificationResponses;
            this.logger.TraceInformation($"Started {nameof(this.ProcessQueuedMeetingNotifications)} method of {nameof(EmailController)}.", traceprops);
            notificationResponses = await this.emailServiceManager.ProcessMeetingNotifications(applicationName, queueNotificationItem).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.ProcessQueuedMeetingNotifications)} method of {nameof(EmailController)}.", traceprops);
            return notificationResponses;
        }

        /// <summary>
        /// Process email notification items synchronously with Graph API.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="meetingInviteItems">Array of email notification items.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("send/{applicationName}")]
        public async Task<IList<NotificationResponse>> SendMeetingInvites(string applicationName, [FromBody] MeetingNotificationItem[] meetingInviteItems)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
            }

            if (meetingInviteItems is null)
            {
                throw new ArgumentNullException(nameof(meetingInviteItems));
            }

            if (meetingInviteItems.Length == 0)
            {
                throw new ArgumentException("Email Notification Items list should not be empty.", nameof(meetingInviteItems));
            }

            IList<NotificationResponse> notificationResponses;
            this.logger.TraceInformation($"Started {nameof(this.SendMeetingInvites)} method of {nameof(MeetingInviteController)}.");

            // Extract the user token from Request to send the email notifications as user or from managed service accounts.
            notificationResponses = await this.emailServiceManager.SendMeetingInvites(applicationName, meetingInviteItems).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.SendMeetingInvites)} method of {nameof(MeetingInviteController)}.");
            return notificationResponses;
        }
    }
}