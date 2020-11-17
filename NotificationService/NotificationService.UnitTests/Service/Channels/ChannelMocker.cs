// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.UnitTests.Service.Channels
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Channels;
    using Moq;
    using NotificationService.Contracts.Models.Web.Response;

    [ExcludeFromCodeCoverage]
    public class ChannelMocker : Channel<WebNotification>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelMocker"/> class.
        /// </summary>
        public ChannelMocker()
        {
            this.ReaderMock = new Mock<ChannelReader<WebNotification>>();
            this.WriterMock = new Mock<ChannelWriter<WebNotification>>();
            this.Writer = this.WriterMock.Object;
            this.Reader = this.ReaderMock.Object;
        }

        public Mock<ChannelWriter<WebNotification>> WriterMock { get; }

        public Mock<ChannelReader<WebNotification>> ReaderMock { get; }
    }
}
