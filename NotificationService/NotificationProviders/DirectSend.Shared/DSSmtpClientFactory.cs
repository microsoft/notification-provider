// <copyright file="DSSmtpClientFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DirectSend
{
    using DirectSend.Models.Configurations;
    using NotificationProviders.Common.Logger;

    /// <summary>
    /// DSSmtpClientFactory.
    /// </summary>
    /// <seealso cref="DirectSend.ISmtpClientFactory" />
    public class DSSmtpClientFactory : ISmtpClientFactory
    {
        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>A <see cref="IDSSmtpClient"/>.</returns>
        public IDSSmtpClient CreateClient(ISmtpConfiguration config, ILogger logger)
        {
            return new DSSmtpClient(config, logger);
        }
    }
}
