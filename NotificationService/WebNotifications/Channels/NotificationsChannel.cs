// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebNotifications.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NotificationService.Contracts.Models.Web.Response;
    using WebNotifications.Channels.Internals;

    /// <summary>
    /// The <see cref="NotificationsChannel"/> class implements producer/consumer functionality for notifications.
    /// </summary>
    public class NotificationsChannel : INotificationsChannel
    {
        /// <summary>
        /// The channel capacity.
        /// </summary>
        private const int ChannelCapacity = 250;

        /// <summary>
        /// The channel.
        /// </summary>
        private readonly Channel<WebNotification> channel;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<NotificationsChannel> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsChannel"/> class.
        /// </summary>
        /// <param name="channelProvider">The instance for <see cref="IChannelProvider"/>.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">logger.</exception>
        public NotificationsChannel(IChannelProvider channelProvider, ILogger<NotificationsChannel> logger)
        {
            if (channelProvider == null)
            {
                throw new ArgumentNullException(nameof(channelProvider));
            }

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            BoundedChannelOptions boundedChannelOptions =
                new BoundedChannelOptions(NotificationsChannel.ChannelCapacity)
                {
                    SingleReader = false,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.DropWrite,
                };

            this.channel = channelProvider.ProvisionBoundedChannel<WebNotification>(boundedChannelOptions);
        }

        /// <summary>
        /// Adds the notification asynchronously.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The instance of <see cref="Task{Boolean}"/> representing an asynchronous operation.</returns>
        public async Task<bool> AddNotificationAsync(WebNotification notification, CancellationToken cancellationToken = default)
        {
            bool itemAdded = false;
            if (notification != null)
            {
                if (!cancellationToken.IsCancellationRequested && this.channel.Writer.TryWrite(notification))
                {
                    Log.ChannelMessageWritten(this.logger, notification.NotificationId);
                    itemAdded = true;
                }
            }

            return itemAdded;
        }

        /// <summary>
        /// Reads all notifications asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The instance of <see cref="IAsyncEnumerable{WebNotification}"/>.</returns>
        public IAsyncEnumerable<WebNotification> ReadAllNotificationsAsync(CancellationToken cancellationToken = default) =>
            this.channel.Reader.ReadAllAsync(cancellationToken);

        /// <summary>
        /// The <see cref="EventIds"/> class defines event identifiers for <see cref="NotificationsChannel"/>.
        /// </summary>
        internal static class EventIds
        {
            /// <summary>
            /// The channel message written event identifier.
            /// </summary>
            public static readonly EventId ChannelMessageWritten = new EventId(100, "ChannelMessageWritten");
        }

        /// <summary>
        /// The <see cref="Log"/> class defines the logging action for <see cref="NotificationsChannel"/>.
        /// </summary>
        private static class Log
        {
            /// <summary>
            /// The channel message written action.
            /// </summary>
            private static readonly Action<ILogger, string, Exception> ChannelMessageWrittenValue = LoggerMessage.Define<string>(
                LogLevel.Information,
                EventIds.ChannelMessageWritten,
                "Notification with {NotificationId} was written to the NotificationsChannel.");

            /// <summary>
            /// Logs the channel message is written.
            /// </summary>
            /// <param name="logger">The logger.</param>
            /// <param name="notificationId">The notification identifier.</param>
            public static void ChannelMessageWritten(ILogger logger, string notificationId) =>
                Log.ChannelMessageWrittenValue(logger, notificationId, null);
        }
    }
}
