// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebNotifications.Channels.Internals
{
    using System.Threading.Channels;

    /// <summary>
    /// The <see cref="IChannelProvider"/> provides mechanism to provide channels.
    /// </summary>
    public interface IChannelProvider
    {
        /// <summary>
        /// Provisions the bounded channel for notifications.
        /// </summary>
        /// <typeparam name="T">The object type for which this channel is provisioned.</typeparam>
        /// <param name="boundedChannelOptions">The instance of <see cref="BoundedChannelOptions"/>.</param>
        /// <returns>The instance of <see cref="Channel{T}"/>.</returns>
        Channel<T> ProvisionBoundedChannel<T>(BoundedChannelOptions boundedChannelOptions);
    }
}
