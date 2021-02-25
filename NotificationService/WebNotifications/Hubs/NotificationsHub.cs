// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebNotifications.Hubs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using NotificationService.BusinessLibrary.Trackers;
    using NotificationService.Common.Configurations;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Models.Trackers;
    using NotificationService.Contracts.Models.Web.Response;
    using WebNotifications.Hubs.Interfaces;

    /// <summary>
    /// THe <see cref="NotificationsHub"/> class represents the notifications SignalR hub for real-time communications.
    /// </summary>
    /// <seealso cref="Hub{INotificationsClient}" />
    [Authorize]
    public class NotificationsHub : Hub<INotificationsClient>
    {
        /// <summary>
        /// The user connection tracker.
        /// </summary>
        private readonly IUserConnectionTracker userConnectionTracker;

        /// <summary>
        /// The registered applications.
        /// </summary>
        private readonly IEnumerable<string> applications;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<NotificationsHub> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsHub"/> class.
        /// </summary>
        /// <param name="userConnectionTracker">The instance of <see cref="IUserConnectionTracker"/>.</param>
        /// <param name="configuration">The instance for <see cref="IConfiguration"/>.</param>
        /// <param name="logger">The instance for <see cref="ILogger{NotificationsHub}"/>.</param>
        /// <exception cref="ArgumentNullException">logger
        /// or
        /// configuration
        /// or
        /// userConnectionTracker.
        /// </exception>
        public NotificationsHub(IUserConnectionTracker userConnectionTracker, IConfiguration configuration, ILogger<NotificationsHub> logger)
        {
            List<ApplicationAccounts> webApplicationAccounts;
            this.userConnectionTracker = userConnectionTracker ?? throw new ArgumentNullException(nameof(userConnectionTracker));
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            webApplicationAccounts = JsonSerializer.Deserialize<List<ApplicationAccounts>>(configuration[ConfigConstants.ApplicationAccountsConfigSectionKey]);
            this.applications = webApplicationAccounts.Select(wa => wa.ApplicationName);
        }

        /// <summary>
        /// Sends the notifications asynchronously.
        /// </summary>
        /// <param name="userObjectIdentifier">The user object identifier.</param>
        /// <param name="notifications">The instance of <see cref="IEnumerable{WebNotification}"/>.</param>
        /// <returns>The instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        public async Task SendNotificationsAsync(string userObjectIdentifier, IEnumerable<WebNotification> notifications)
        {
            this.logger.LogInformation($"Started {nameof(this.SendNotificationsAsync)} method of {nameof(NotificationsHub)}.");
            if (string.IsNullOrWhiteSpace(userObjectIdentifier))
            {
                throw new ArgumentException("Invalid user identifier.", nameof(userObjectIdentifier));
            }

            if (notifications == null)
            {
                throw new ArgumentNullException(nameof(notifications));
            }

            if (!notifications.Any())
            {
                this.logger.LogWarning("There are no notifications to send.");
            }
            else
            {
                await this.Clients.User(userObjectIdentifier).ReceiveNotificationsAsync(notifications).ConfigureAwait(false);
                this.logger.LogInformation($"Sending notifications to user with identifier '{userObjectIdentifier}'.");
            }

            this.logger.LogInformation($"Finished {nameof(this.SendNotificationsAsync)} method of {nameof(NotificationsHub)}.");
        }

        /// <summary>
        /// Broadcasts the notification asynchronously.
        /// </summary>
        /// <param name="notification">The instance of <see cref="WebNotification"/>.</param>
        /// <returns>The instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        public async Task BroadcastNotificationAsync(WebNotification notification)
        {
            this.logger.LogInformation($"Started {nameof(this.BroadcastNotificationAsync)} method of {nameof(NotificationsHub)}.");
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            await this.Clients.All.ReceiveNotificationAsync(notification).ConfigureAwait(false);
            this.logger.LogInformation($"Finished {nameof(this.BroadcastNotificationAsync)} method of {nameof(NotificationsHub)}.");
        }

        /// <summary>
        /// Receives the name of the application.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <exception cref="ArgumentException">Missing application name. - applicationName.</exception>
        /// <exception cref="HubException">Invalid application name.</exception>
        public void ReceiveApplicationName(string applicationName)
        {
            this.logger.LogInformation($"Started {nameof(this.ReceiveApplicationName)} method of {nameof(NotificationsHub)}.");
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("Missing application name.", nameof(applicationName));
            }

            this.logger.LogInformation($"Received application name '{applicationName}' for '{this.Context.ConnectionId}' connection with '{this.Context.UserIdentifier}'");
            if (!this.applications.Any(appName => appName.Equals(applicationName, StringComparison.OrdinalIgnoreCase)))
            {
                this.logger.LogError($"The application '{applicationName}' is not registered with the service.");
                throw new HubException("Invalid application name.");
            }

            this.userConnectionTracker.SetConnectionApplicationName(this.Context.UserIdentifier, new UserConnectionInfo(this.Context.ConnectionId, applicationName));
            this.logger.LogInformation($"Finished {nameof(this.ReceiveApplicationName)} method of {nameof(NotificationsHub)}.");
        }

        /// <summary>
        /// Called when a new connection is established with the hub.
        /// </summary>
        /// <returns>The instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        public override async Task OnConnectedAsync()
        {
            this.logger.LogInformation($"New user connected to '{nameof(NotificationsHub)}' with identifier '{this.Context.UserIdentifier}'");
            this.userConnectionTracker.SetConnectionInfo(this.Context.UserIdentifier, new UserConnectionInfo(this.Context.ConnectionId, UserConnectionTracker.BrowserApplication));
            await base.OnConnectedAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Called when a connection with the hub is terminated.
        /// </summary>
        /// <param name="exception">The instance for <see cref="Exception"/>.</param>
        /// <returns>The instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            this.logger.LogInformation($"The User disconnected from '{nameof(NotificationsHub)}' with identifier '{this.Context.UserIdentifier}'.");
            if (exception != null)
            {
                this.logger.LogError(exception, $"The user with identifier '{this.Context.UserIdentifier}' disconnected with error.");
            }

            this.userConnectionTracker.RemoveConnectionInfo(this.Context.UserIdentifier, this.Context.ConnectionId);
            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }
    }
}
