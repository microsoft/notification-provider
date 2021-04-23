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
    using Newtonsoft.Json;
    using NotificationHandler.Controllers.V1;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Logger;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Extensions;
    using NotificationService.Contracts.Models;
    using NotificationService.Contracts.Models.Request;
    using NotificationService.SvCommon.Attributes;

    /// <summary>
    /// Controller to handle meeting invite notifications.
    /// </summary>
    [Route("v1/meetinginvite")]
    [Authorize(Policy = ApplicationConstants.AppNameAuthorizePolicy)]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class MeetingInviteController : BaseController
    {
        /// <summary>
        /// Instance of <see cref="IEmailHandlerManager"/>.
        /// </summary>
        private readonly IEmailHandlerManager emailHandlerManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingInviteController"/> class.
        /// </summary>
        /// <param name="emailHandlerManager">An instance of <see cref="EmailHandlerManager"/>.</param>
        /// <param name="templateManager">An instance of <see cref="MailTemplateManager"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        /// <param name="correlationProvider">An instance of <see cref="ICorrelationProvider"/>.</param>
        public MeetingInviteController(IEmailHandlerManager emailHandlerManager, ILogger logger)
            : base(logger)
        {
            this.emailHandlerManager = emailHandlerManager ?? throw new System.ArgumentNullException(nameof(emailHandlerManager));
        }

        /// <summary>
        /// Queue meeting invite  notification items for asynchronous processing.
        /// </summary>
        /// <param name="applicationName">Application sourcing the meeting invite  notification.</param>
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

        /// <summary>
        /// Resend meeting notification items.
        /// </summary>
        /// <param name="applicationName">Application sourcing the meeting invite notification.</param>
        /// <param name="notificationIds">Array of email notification ids.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("resend/{applicationName}")]
        public async Task<IActionResult> ResendMeetingInvites(string applicationName, [FromBody] string[] notificationIds)
        {
            var traceProps = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                this.LogAndThrowArgumentNullException("Application Name cannot be null or empty.", nameof(applicationName), traceProps);
            }

            traceProps[AIConstants.Application] = applicationName;
            if (notificationIds is null || notificationIds.Length == 0)
            {
                this.LogAndThrowArgumentNullException("Meeting Notifications Ids list should not be null/empty", nameof(notificationIds), traceProps);
            }

            traceProps[AIConstants.NotificationIds] = string.Join(',', notificationIds);

            IList<NotificationResponse> notificationResponses;
            this.logger.TraceInformation($"Started {nameof(this.ResendMeetingInvites)} method of {nameof(MeetingInviteController)}.", traceProps);
            notificationResponses = await this.emailHandlerManager.ResendNotifications(applicationName, notificationIds, NotificationType.Meet).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.ResendMeetingInvites)} method of {nameof(MeetingInviteController)}.", traceProps);
            return this.Accepted(notificationResponses);
        }

        /// <summary>
        /// Resend meeting notification items by Date Range.
        /// </summary>
        /// <param name="applicationName">Application sourcing the meeting invite  notification.</param>
        /// <param name="dateRange">Date Range to resubmit the notifications.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = ApplicationConstants.BearerAuthenticationScheme)]
        [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
        [Route("resend/{applicationName}/bydaterange")]
        public async Task<IActionResult> ResendMeetingNotificationsByDateRange(string applicationName, [FromBody] DateTimeRange dateRange)
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
            this.logger.TraceInformation($"Started {nameof(this.ResendMeetingNotificationsByDateRange)} method of {nameof(MeetingInviteController)}.", traceProps);
            notificationResponses = await this.emailHandlerManager.ResendMeetingNotificationsByDateRange(applicationName, dateRange).ConfigureAwait(false);
            this.logger.TraceInformation($"Finished {nameof(this.ResendMeetingNotificationsByDateRange)} method of {nameof(MeetingInviteController)}.", traceProps);
            return this.Accepted(notificationResponses);
        }
    }
}