// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebNotifications.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Contracts.Models.Web.Request;
    using NotificationService.Contracts.Models.Web.Response;
    using NotificationService.SvCommon.APIConstants;
    using NotificationService.SvCommon.Attributes;
    using WebNotifications.Channels;
    using ApplicationConstants = NotificationService.Common.ApplicationConstants;

    /// <summary>
    /// The <see cref="NotificationsController"/> class provides API for web notifications management.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [Route("v1/notifications")]
    [Authorize(Policy = ApplicationConstants.AppNameAuthorizePolicy)]
    [Authorize(Policy = ApplicationConstants.AppIdAuthorizePolicy)]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class NotificationsController : ControllerBase
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<NotificationsController> logger;

        /// <summary>
        /// The notifications manager.
        /// </summary>
        private readonly INotificationsManager notificationsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsController"/> class.
        /// </summary>
        /// <param name="notificationsManager">The instance for <see cref="INotificationsManager"/>.</param>
        /// <param name="logger">The instance for <see cref="ILogger{NotificationsController}"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// logger
        /// or
        /// notificationsManager.
        /// </exception>
        public NotificationsController(INotificationsManager notificationsManager, ILogger<NotificationsController> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.notificationsManager = notificationsManager ?? throw new ArgumentNullException(nameof(notificationsManager));
        }

        /// <summary>
        /// Gets the root activity identifier.
        /// </summary>
        /// <value>
        /// The root activity identifier.
        /// </value>
        protected string RootActivityId => this.HttpContext.Request.Headers["x-ms-root-activity-id"].Any() ?
            this.HttpContext.Request.Headers["x-ms-root-activity-id"][0] : Guid.NewGuid().ToString();

        /// <summary>
        /// Gets the notifications for a user.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns>The instance of <see cref="Task{IActionResult}"/> representing an asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Application name is mandatory.</exception>
        [ProducesResponseType(200, Type = typeof(WebNotificationResponse))]
        [HttpGet("{applicationName}")]
        public async Task<IActionResult> GetNotifications(string applicationName)
        {
            IActionResult result = null;
            WebNotificationResponse notificationResponse = null;
            using (this.logger.BeginScope($"RootActivityId: {this.RootActivityId}"))
            {
                this.logger.LogInformation($"Started {nameof(this.GetNotifications)} method of {nameof(NotificationsController)}.");
                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    throw new ArgumentException("Application name is mandatory.", nameof(applicationName));
                }

                notificationResponse = await this.notificationsManager.DeliverNotificationsAsync(applicationName, this.HttpContext.User.FindFirst(ClaimTypeConstants.ObjectIdentifier)?.Value).ConfigureAwait(false);
                result = this.Ok(notificationResponse);
                this.logger.LogInformation($"Finished {nameof(this.GetNotifications)} method of {nameof(NotificationsController)}.");
            }

            return result;
        }

        /// <summary>
        /// Posts the new notifications.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="webNotificationRequestsContainer">The instance of <see cref="WebNotificationRequestItemsContainer"/>.</param>
        /// <param name="notificationsChannel">The instance for <see cref="INotificationsChannel"/>.</param>
        /// <returns>The instance of <see cref="Task{IActionResult}"/> representing an asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Application name is mandatory.</exception>
        /// <exception cref="ArgumentNullException">WebNotification Request Container and Channel are mandatory.</exception>
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [HttpPost("{applicationName}")]
        public async Task<IActionResult> PostNotifications(string applicationName, [FromBody] WebNotificationRequestItemsContainer webNotificationRequestsContainer, [FromServices] INotificationsChannel notificationsChannel)
        {
            IActionResult result = null;
            using (this.logger.BeginScope($"RootActivityId: {this.RootActivityId}"))
            {
                this.logger.LogInformation($"Started {nameof(this.PostNotifications)} method of {nameof(NotificationsController)}.");
                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    throw new ArgumentException("Application name is mandatory.", nameof(applicationName));
                }

                if (webNotificationRequestsContainer is null)
                {
                    throw new ArgumentNullException(nameof(webNotificationRequestsContainer));
                }

                if (notificationsChannel == null)
                {
                    throw new ArgumentNullException(nameof(notificationsChannel));
                }

                IEnumerable<WebNotification> notifications = await this.notificationsManager.ProcessNotificationsAsync(applicationName, webNotificationRequestsContainer.Notifications).ConfigureAwait(false);
                foreach (var notification in notifications.ToList())
                {
                    _ = notificationsChannel.AddNotificationAsync(notification);
                }

                result = this.Accepted();
                this.logger.LogInformation($"Finished {nameof(this.PostNotifications)} method of {nameof(NotificationsController)}.");
            }

            return result;
        }

        /// <summary>
        /// Updates the notifications' read status.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="notificationIdsContainer">The instance of <see cref="NotificationIdsContainer"/>.</param>
        /// <returns>The instance of <see cref="Task{IActionResult}"/> representing an asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// notifcationIdsContainer.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Application name is mandatory.
        /// </exception>
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [HttpPost("read/{applicationName}")]
        public async Task<IActionResult> UpdateNotificationsReadStatus(string applicationName, [FromBody] NotificationIdsContainer notificationIdsContainer)
        {
            IActionResult result = null;
            using (this.logger.BeginScope($"RootActivityId: {this.RootActivityId}"))
            {
                this.logger.LogInformation($"Started {nameof(this.UpdateNotificationsReadStatus)} method of {nameof(NotificationsController)}.");
                if (notificationIdsContainer is null)
                {
                    throw new ArgumentNullException(nameof(notificationIdsContainer));
                }

                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    throw new ArgumentException("Application name is mandatory.", nameof(applicationName));
                }

                await this.notificationsManager.MarkNotificationsAsReadAsync(applicationName, notificationIdsContainer.NotificationIds).ConfigureAwait(false);
                result = this.NoContent();
                this.logger.LogInformation($"Finished {nameof(this.UpdateNotificationsReadStatus)} method of {nameof(NotificationsController)}.");
            }

            return result;
        }

        /// <summary>
        /// Reads the notifications status asynchronously.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="notificationTrackingIdsContainer">The notification tracking ids container.</param>
        /// <returns>The instance of <see cref="Task{IActionResult}"/> representing an asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Application name is mandatory. - applicationName.</exception>
        /// <exception cref="ArgumentNullException">notificationTrackingIdsContainer.</exception>
        [ProducesResponseType(typeof(WebNotificationStatusResponse), 200)]
        [ProducesResponseType(400)]
        [HttpPost("status/{applicationName}")]
        public async Task<IActionResult> ReadNotificationsStatusAsync(string applicationName, [FromBody] NotificationTrackingIdsContainer notificationTrackingIdsContainer)
        {
            IActionResult result = null;
            using (this.logger.BeginScope($"RootActivityId: {this.RootActivityId}"))
            {
                this.logger.LogInformation($"Started {nameof(this.ReadNotificationsStatusAsync)} method of {nameof(NotificationsController)}.");
                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    throw new ArgumentException("Application name is mandatory.", nameof(applicationName));
                }

                if (notificationTrackingIdsContainer == null)
                {
                    throw new ArgumentNullException(nameof(notificationTrackingIdsContainer));
                }

                WebNotificationStatusResponse statusResponse = await this.notificationsManager.LoadNotificationStatusAsync(applicationName, notificationTrackingIdsContainer.TrackingIds).ConfigureAwait(false);
                result = this.Ok(statusResponse);
                this.logger.LogInformation($"Finished {nameof(this.ReadNotificationsStatusAsync)} method of {nameof(NotificationsController)}.");
            }

            return result;
        }
    }
}