// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebNotifications.Carriers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;
    using NotificationService.BusinessLibrary.Trackers;
    using NotificationService.Contracts.Models.Web.Response;
    using WebNotifications.Carriers.Interfaces;
    using WebNotifications.Hubs;
    using WebNotifications.Hubs.Interfaces;

    /// <summary>
    /// The <see cref="WebNotificationsCarrier"/> class implements mechanism to carry notifications to the recipients in real-time.
    /// </summary>
    /// <seealso cref="IWebNotificationsCarrier" />
    public class WebNotificationsCarrier : IWebNotificationsCarrier
    {
        /// <summary>
        /// The hub context.
        /// </summary>
        private readonly IHubContext<NotificationsHub, INotificationsClient> hubContext;

        /// <summary>
        /// The user connections reader.
        /// </summary>
        private readonly IUserConnectionsReader userConnectionsReader;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<WebNotificationsCarrier> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebNotificationsCarrier"/> class.
        /// </summary>
        /// <param name="hubContext">The hub context.</param>
        /// <param name="userConnectionsReader">The instance for <see cref="IUserConnectionsReader"/>.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">
        /// hubContext
        /// or
        /// logger.
        /// </exception>
        public WebNotificationsCarrier(IHubContext<NotificationsHub, INotificationsClient> hubContext, IUserConnectionsReader userConnectionsReader, ILogger<WebNotificationsCarrier> logger)
        {
            this.hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            this.userConnectionsReader = userConnectionsReader ?? throw new ArgumentNullException(nameof(userConnectionsReader));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> SendAsync(IEnumerable<WebNotification> webNotifications)
        {
            IEnumerable<string> deliveredNotificationIds = new List<string>();
            this.logger.LogInformation($"Started {nameof(this.SendAsync)} method of {nameof(WebNotificationsCarrier)}.");
            if (webNotifications != null && webNotifications.Any())
            {
                deliveredNotificationIds = await this.SendInternalAsync(webNotifications).ConfigureAwait(false);
            }
            else
            {
                this.logger.LogWarning("There are no notifications to deliver.");
            }

            this.logger.LogInformation($"Finished {nameof(this.SendAsync)} method of {nameof(WebNotificationsCarrier)}.");
            return deliveredNotificationIds;
        }

        /// <summary>
        /// Sends the notifications internal asynchronously.
        /// </summary>
        /// <param name="webNotifications">The instance of <see cref="IEnumerable{WebNotification}"/>.</param>
        /// <returns>The instance of <see cref="Task{T}"/> where <c>T</c> being <see cref="List{String}"/>.</returns>
        private async Task<List<string>> SendInternalAsync(IEnumerable<WebNotification> webNotifications)
        {
            Debug.Assert(webNotifications != null, "Missing notifications.");
            IEnumerable<string> connectionIds;
            List<string> deliveryNotificationIds = new List<string>();
            foreach (WebNotification webNotification in webNotifications)
            {
                try
                {
                    connectionIds = this.userConnectionsReader.GetUserConnectionIds(webNotification.Recipient.ObjectIdentifier, webNotification.Application);
                    if (connectionIds?.Any() ?? false)
                    {
                        await this.hubContext.Clients.Clients(connectionIds.ToList().AsReadOnly())
                            .ReceiveNotificationAsync(webNotification).ConfigureAwait(false);
                        deliveryNotificationIds.Add(webNotification.NotificationId);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Failure sending notification to user with identifier '{webNotification.Recipient.ObjectIdentifier}'");
                }
            }

            return deliveryNotificationIds;
        }
    }
}
