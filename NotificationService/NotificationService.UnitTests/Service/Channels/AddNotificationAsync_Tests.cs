// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Service.Channels
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using NotificationService.Contracts.Models.Web.Response;
    using NUnit.Framework;

    [ExcludeFromCodeCoverage]
    public class AddNotificationAsync_Tests : NotificationsChannelBaseTests
    {
        [SetUp]
        public void Setup() => this.SetupFirst();

        [Test]
        public async Task AddNotificationAsync_NullNotification()
        {
            var result = await this.notificationsChannel.AddNotificationAsync(null, CancellationToken.None).ConfigureAwait(false);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task AddNotificationAsync_ValidInputs()
        {
            _ = this.channelMocker.WriterMock.Setup(wr => wr.TryWrite(It.IsAny<WebNotification>())).Returns(true);
            var result = await this.notificationsChannel.AddNotificationAsync(new WebNotification { NotificationId = Guid.NewGuid().ToString() }).ConfigureAwait(false);
            Assert.IsTrue(result);
        }
    }
}
