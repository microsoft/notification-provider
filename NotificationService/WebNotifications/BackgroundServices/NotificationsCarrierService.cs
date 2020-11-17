// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebNotifications.BackgroundServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Contracts.Entities.Web;
    using NotificationService.Contracts.Models.Web.Response;
    using WebNotifications.Carriers.Interfaces;
    using WebNotifications.Channels;

    /// <summary>
    /// The <see cref="NotificationsCarrierService"/> class implements a notification delivery service.
    /// </summary>
    /// <seealso cref="BackgroundService" />
    public class NotificationsCarrierService : BackgroundService
    {
        /// <summary>
        /// The web notifications carrier.
        /// </summary>
        private readonly IWebNotificationsCarrier webNotificationsCarrier;

        /// <summary>
        /// The notifications channel.
        /// </summary>
        private readonly INotificationsChannel notificationsChannel;

        /// <summary>
        /// The services provider.
        /// </summary>
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<NotificationsCarrierService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsCarrierService"/> class.
        /// </summary>
        /// <param name="services">The instance for <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">
        /// services
        /// or
        /// logger.
        /// </exception>
        public NotificationsCarrierService(IServiceProvider services, ILogger<NotificationsCarrierService> logger)
        {
            this.serviceProvider = services ?? throw new ArgumentNullException(nameof(services));
            this.webNotificationsCarrier = services.GetRequiredService<IWebNotificationsCarrier>();
            this.notificationsChannel = services.GetRequiredService<INotificationsChannel>();
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the web notification delivery operation.
        /// </summary>
        /// <remarks>
        /// Note: This method should not contain major logic.
        /// </remarks>
        /// <param name="stoppingToken">Triggered when <see cref="IHostedService.StopAsync(CancellationToken)" /> is called.</param>
        /// <returns>The instance of <see cref="Task"/> representing an asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            INotificationDelivery notificationDelivery;
            List<string> notificationIds = new List<string>();
            IEnumerable<string> deliveredNotificationIds;
            _ = stoppingToken.Register(() =>
              {
                  this.logger.LogWarning("The notification carrier service is stopping.");
              });

            // Force method to run asynchronously.
            await Task.Yield();
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await foreach (var notification in this.notificationsChannel.ReadAllNotificationsAsync(stoppingToken))
                    {
                        try
                        {
                            deliveredNotificationIds = await this.webNotificationsCarrier.SendAsync(new List<WebNotification> { notification }).ConfigureAwait(false);
                            notificationIds.AddRange(deliveredNotificationIds);
                            if (notificationIds.Count >= 10)
                            {
                                using (var scope = this.serviceProvider.CreateScope())
                                {
                                    try
                                    {
                                        notificationDelivery = scope.ServiceProvider.GetRequiredService<INotificationDelivery>();
                                        _ = notificationDelivery.MarkNotificationDeliveredAsync(notificationIds, NotificationDeliveryChannel.Web, throwExceptions: false);
                                    }
                                    finally
                                    {
                                        notificationIds.Clear();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(ex, "Error delivering notifications.");
                        }
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                this.logger.LogError(ex, "The notification carrier service operation canceled.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "The notification carrier service operation encountered unhandled exception.");
            }
        }
    }
}
