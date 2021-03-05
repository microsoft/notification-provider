// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationHandler.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Extensions;
    using NotificationService.Contracts.Models;
    using NotificationService.SvCommon.Attributes;

    /// <summary>
    /// Controller to handle email notifications.
    /// </summary>
    [Route("v1/meetinginvite")]
    [Authorize(Policy = ApplicationConstants.AppNameAuthorizePolicy)]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class MeetingInviteController : Controller
    {
        /// <summary>
        /// Instance of <see cref="IEmailHandlerManager"/>.
        /// </summary>
        private readonly IEmailHandlerManager emailHandlerManager;

        /// <summary>
        /// Instance of <see cref="ILogger"/>.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingInviteController"/> class.
        /// </summary>
        /// <param name="emailHandlerManager">An instance of <see cref="EmailManager"/>.</param>
        /// <param name="templateManager">An instance of <see cref="MailTemplateManager"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        /// <param name="correlationProvider">An instance of <see cref="ICorrelationProvider"/>.</param>
        public MeetingInviteController(IEmailHandlerManager emailHandlerManager, ILogger logger)
        {
            this.emailHandlerManager = emailHandlerManager ?? throw new System.ArgumentNullException(nameof(emailHandlerManager));
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Queue email notification items for asynchronous processing.
        /// </summary>
        /// <param name="applicationName">Application sourcing the email notification.</param>
        /// <param name="meetingNotificationItems">Array of email notification items.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("queue/{applicationName}")]
        public async Task<IActionResult> QueueMeetingNotifications(string applicationName, [FromBody] MeetingNotificationItem[] meetingNotificationItems)
        {
            var traceProps = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("Application Name cannot be null or empty.", nameof(applicationName));
            }

            if (meetingNotificationItems is null)
            {
                throw new ArgumentNullException(nameof(meetingNotificationItems));
            }

            if (meetingNotificationItems.Length == 0)
            {
                this.logger.WriteException(new System.ArgumentException("Meeting Notification Items list should not be empty", nameof(meetingNotificationItems)), traceProps);
                throw new System.ArgumentException("Meeting Notification Items list should not be empty", nameof(meetingNotificationItems));
            }

            traceProps[AIConstants.Application] = applicationName;
            IList<NotificationResponse> notificationResponses = new List<NotificationResponse>();
            foreach (var item in meetingNotificationItems)
            {
                var res = item.ValidateMeetingInvite();
                notificationResponses.Add(
                    new NotificationResponse
                    {
                        TrackingId = item.TrackingId,
                        Status = res.Result ? NotificationItemStatus.NotQueued : NotificationItemStatus.Invalid,
                        ErrorMessage = res.Message,
                    });
            }

            if (notificationResponses.Any(x => x.Status == NotificationItemStatus.Invalid))
            {
                return this.BadRequest(notificationResponses);
            }

            this.logger.TraceInformation($"Started {nameof(this.QueueMeetingNotifications)} method of {nameof(MeetingInviteController)}.", traceProps);
            notificationResponses = await this.emailHandlerManager.QueueMeetingNotifications(applicationName, meetingNotificationItems).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.QueueMeetingNotifications)} method of {nameof(MeetingInviteController)}.", traceProps);
            return this.Accepted(notificationResponses);
        }
    }
}