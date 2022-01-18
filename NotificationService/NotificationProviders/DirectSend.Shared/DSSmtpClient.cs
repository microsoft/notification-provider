// <copyright file="DSSmtpClient.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DirectSend
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DirectSend.Models.Configurations;
    using MailKit;
    using MailKit.Net.Smtp;
    using MimeKit;
    using NotificationProviders.Common.Logger;

    /// <summary>
    /// IDSSmtpClient.
    /// </summary>
    /// <seealso cref="IDSSmtpClient" />
    public class DSSmtpClient : IDSSmtpClient
    {
        private readonly ILogger logger;
        private readonly int timeout = 5;
        private readonly ISmtpConfiguration config;

        private bool disposedValue; // To detect redundant calls

        private DateTime lastSend;
        private SmtpClient smtpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DSSmtpClient"/> class.
        /// </summary>
        /// <param name="config">ISmtpConfiguration.</param>
        /// <param name="logger">ILogger.</param>
        public DSSmtpClient(ISmtpConfiguration config, ILogger logger)
        {
            this.config = config;
            this.logger = logger;
            this.lastSend = DateTime.Now;
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="traceProps">The trace props.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendAsync(MimeMessage message, Dictionary<string, string> traceProps, ITransferProgress progress = null, CancellationToken cancellationToken = default)
        {
            TimeSpan span = DateTime.Now - this.lastSend;

            if (this.smtpClient == null || span.TotalMinutes > this.timeout)
            {
                this.Refresh(traceProps);
            }

            await this.smtpClient.SendAsync(message).ConfigureAwait(false);

            this.lastSend = DateTime.Now;
        }

        /// <summary>
        /// Refreshes the specified trace props.
        /// </summary>
        /// <param name="traceProps">The trace props.</param>
        public void Refresh(Dictionary<string, string> traceProps)
        {
            if (this.smtpClient != null)
            {
                this.smtpClient.Dispose();
            }

            this.CreateClient(traceProps);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.smtpClient != null)
                    {
                        this.smtpClient.Dispose();
                    }
                }

                this.disposedValue = true;
            }
        }

        private void CreateClient(Dictionary<string, string> traceProps)
        {
            this.smtpClient = new SmtpClient();
            try
            {
                this.logger.TraceInformation("Initializing email client.", traceProps);
                this.smtpClient.Connect(this.config.SmtpServer, this.config.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                _ = this.smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");

                this.logger.WriteMetric("SmtpClientPool_SmtpConnectionOpen", 1, traceProps);
            }
            catch (SmtpCommandException ex)
            {
                SmptInitializationException e = new SmptInitializationException($"Initialization Failure: {ex.Message}, {ex.HelpLink}", ex);
                this.logger.WriteException(e, traceProps);
                throw e;
            }
            catch (SmtpProtocolException ex)
            {
                SmptInitializationException e = new SmptInitializationException($"Initialization Failure: {ex.Message}, {ex.HelpLink}", ex);
                this.logger.WriteException(e, traceProps);
                throw e;
            }
        }
    }
}
