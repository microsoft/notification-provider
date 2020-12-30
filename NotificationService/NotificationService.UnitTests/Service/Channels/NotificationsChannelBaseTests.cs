// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Service.Channels
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Channels;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NotificationService.Contracts.Models.Web.Response;
    using WebNotifications.Channels;
    using WebNotifications.Channels.Internals;

    /// <summary>
    /// Test Base Class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NotificationsChannelBaseTests
    {
        /// <summary>
        /// The channel mock.
        /// </summary>
        protected ChannelMocker channelMocker;

        /// <summary>
        /// The channel provider mock.
        /// </summary>
        protected Mock<IChannelProvider> channelProviderMock;

        /// <summary>
        /// The notifications channel.
        /// </summary>
        protected NotificationsChannel notificationsChannel;

        /// <summary>
        /// Setups the first.
        /// </summary>
        public void SetupFirst()
        {
            this.channelProviderMock = new Mock<IChannelProvider>();
            this.channelMocker = new ChannelMocker();
            _ = this.channelProviderMock.Setup(cp => cp.ProvisionBoundedChannel<WebNotification>(It.IsAny<BoundedChannelOptions>())).Returns(this.channelMocker);
            this.notificationsChannel = new NotificationsChannel(this.channelProviderMock.Object, NullLogger<NotificationsChannel>.Instance);
        }
    }
}
