// <copyright file="ISmtpClientFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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