// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Service.Channels
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NUnit.Framework;
    using WebNotifications.Channels;

    /// <summary>
    /// Test Base Class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Ctor_Tests : NotificationsChannelBaseTests
    {
        /// <summary>
        /// Ctors the null channel provider.
        /// </summary>
        [Test]
        public void Ctor_NullChannelProvider()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new NotificationsChannel(null, NullLogger<NotificationsChannel>.Instance));
            Assert.IsTrue(ex.ParamName.Equals("channelProvider", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Ctors the null logger.
        /// </summary>
        [Test]
        public void Ctor_NullLogger()
        {
            this.SetupFirst();
            var ex = Assert.Throws<ArgumentNullException>(() => new NotificationsChannel(this.channelProviderMock.Object, null));
            Assert.IsTrue(ex.ParamName.Equals("logger", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Reads all notifications asynchronous check.
        /// </summary>
        [Test]
        public void ReadAllNotificationsAsync_Check()
        {
            this.SetupFirst();
            this.channelMocker.ReaderMock.Setup(rc => rc.ReadAllAsync(It.IsAny<CancellationToken>())).Verifiable();
            _ = this.notificationsChannel.ReadAllNotificationsAsync(CancellationToken.None);
            this.channelMocker.ReaderMock.Verify(rc => rc.ReadAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
