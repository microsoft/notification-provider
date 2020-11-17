// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DirectSend
{
    using DirectSend.Models.Configurations;
    using NotificationProviders.Common.Logger;

    /// <summary>
    /// ISmtpClientFactory.
    /// </summary>
    public interface ISmtpClientFactory
    {
        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>A <see cref="IDSSmtpClient"/>.</returns>
        IDSSmtpClient CreateClient(ISmtpConfiguration config, ILogger logger);
    }
}