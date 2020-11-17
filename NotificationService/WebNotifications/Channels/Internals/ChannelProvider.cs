// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebNotifications.Channels.Internals
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Channels;

    /// <summary>
    /// The <see cref="ChannelProvider"/> class implements the mechanism to provision the channels.
    /// </summary>
    /// <seealso cref="IChannelProvider" />
    [ExcludeFromCodeCoverage]
    internal class ChannelProvider : IChannelProvider
    {
        /// <inheritdoc cref="IChannelProvider"/>
        public Channel<T> ProvisionBoundedChannel<T>(BoundedChannelOptions boundedChannelOptions)
        {
            // This method and interface introduced to increase testability of the code.
            if (boundedChannelOptions == null)
            {
                throw new ArgumentNullException(nameof(boundedChannelOptions));
            }

            return Channel.CreateBounded<T>(boundedChannelOptions);
        }
    }
}
