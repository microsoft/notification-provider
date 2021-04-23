// <copyright file="IDSSmtpClient.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DirectSend
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MailKit;
    using MimeKit;

    /// <summary>
    /// IDSSmtpClient.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IDSSmtpClient : IDisposable
    {
        /// <summary>
        /// Refreshes the specified log event.
        /// </summary>
        /// <param name="traceProps">traceProps.</param>
        void Refresh(Dictionary<string, string> traceProps);

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="traceProperties">The traceProperties.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task SendAsync(MimeMessage message, Dictionary<string, string> traceProperties, ITransferProgress progress = null, CancellationToken cancellationToken = default);
    }
}
