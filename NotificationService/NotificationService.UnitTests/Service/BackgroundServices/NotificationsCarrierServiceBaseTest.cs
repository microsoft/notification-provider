// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Service.BackgroundServices
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using WebNotifications.BackgroundServices;
    using WebNotifications.Carriers.Interfaces;
    using WebNotifications.Channels;

    /// <summary>
    /// Test class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NotificationsCarrierServiceBaseTest
    {
        /// <summary>
        /// The notifications channel mock.
        /// </summary>
        private Mock<INotificationsChannel> notificationsChannelMock;

        /// <summary>
        /// The notifications carrier mock.
        /// </summary>
        private Mock<IWebNotificationsCarrier> notificationsCarrierMock;

        /// <summary>
        /// The service provider mock.
        /// </summary>
        private Mock<IServiceProvider> serviceProviderMock;

        /// <summary>
        /// The notifications carrier service.
        /// </summary>
        private NotificationsCarrierService notificationsCarrierService;

        /// <summary>
        /// Setups the base.
        /// </summary>
        public void SetupBase()
        {
            var serviceScope = new Mock<IServiceScope>();
            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            this.notificationsChannelMock = new Mock<INotificationsChannel>();
            this.notificationsCarrierMock = new Mock<IWebNotificationsCarrier>();
            this.serviceProviderMock = new Mock<IServiceProvider>();
            _ = serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);
            _ = serviceScope.Setup(x => x.ServiceProvider).Returns(this.serviceProviderMock.Object);
            _ = this.serviceProviderMock.Setup(sp => sp.GetService(typeof(INotificationsChannel))).Returns(this.notificationsChannelMock.Object);
            _ = this.serviceProviderMock.Setup(sp => sp.GetService(typeof(IWebNotificationsCarrier))).Returns(this.notificationsCarrierMock.Object);
            _ = this.serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactory.Object);
            this.notificationsCarrierService = new NotificationsCarrierService(this.serviceProviderMock.Object, NullLogger<NotificationsCarrierService>.Instance);
        }
    }
}
