// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Service.BackgroundServices
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using WebNotifications.BackgroundServices;
    using NUnit.Framework;

    /// <summary>
    /// Ctor Tests.
    /// </summary>
    /// <seealso cref="NotificationService.UnitTests.Service.BackgroundServices.NotificationsCarrierServiceBaseTest" />
    public class Ctor_Tests : NotificationsCarrierServiceBaseTest
    {
        /// <summary>
        /// Ctors the null logger.
        /// </summary>
        [Test]
        public void Ctor_NullLogger()
        {
            this.SetupBase();
            var ex = Assert.Throws<ArgumentNullException>(() => new NotificationsCarrierService(this.serviceProviderMock.Object, null));
            Assert.IsTrue(ex.ParamName.Equals("logger", StringComparison.Ordinal));
        }

        /// <summary>
        /// Ctors the valid input.
        /// </summary>
        [Test]
        public void Ctor_ValidInput()
        {
            this.SetupBase();
            using var service = new NotificationsCarrierService(this.serviceProviderMock.Object, NullLogger<NotificationsCarrierService>.Instance);
            Assert.IsTrue(service.GetType().FullName.Equals(typeof(NotificationsCarrierService).FullName));
        }

        /// <summary>
        /// Exceptions the in exceute asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task ExceptionInExceuteAsync()
        {
            this.SetupBase();
            _ = this.notificationsChannelMock.Setup(ncm => ncm.ReadAllNotificationsAsync(It.IsAny<CancellationToken>())).Throws(new Exception("Test exception."));
            using var service = new NotificationsCarrierService(this.serviceProviderMock.Object, NullLogger<NotificationsCarrierService>.Instance);
            await service.StartAsync(default);

            // Time to let service to come in action.
            await Task.Delay(100);

            this.notificationsChannelMock.Verify(ncm => ncm.ReadAllNotificationsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
