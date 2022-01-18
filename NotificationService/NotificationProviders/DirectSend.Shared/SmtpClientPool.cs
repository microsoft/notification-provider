// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DirectSend
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DirectSend.Models.Configurations;
    using NotificationProviders.Common.Logger;

    /// <summary>
    /// SmtpClientPool.
    /// </summary>
    /// <seealso cref="DirectSend.ISmtpClientPool" />
    public class SmtpClientPool : ISmtpClientPool
    {
        private const int MaxClientCount = 10;

        private static readonly ConcurrentQueue<IDSSmtpClient> ClientList = new ConcurrentQueue<IDSSmtpClient>();
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        private static int currentClientCount;

        private readonly ILogger logger;
        private readonly ISmtpConfiguration config;
        private readonly ISmtpClientFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpClientPool"/> class.
        /// </summary>
        /// <param name="config">SmtpConfiguration.</param>
        /// <param name="logger">logger.</param>
        /// <param name="factory">SmtpClientFactory.</param>
        public SmtpClientPool(ISmtpConfiguration config, ILogger logger, ISmtpClientFactory factory)
        {
            this.logger = logger;
            this.config = config;
            this.factory = factory;
        }

        /// <inheritdoc/>
        public string EndPoint
        {
            get
            {
                return this.config.SmtpServer;
            }
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <param name="traceProperties">The trace properties.</param>
        /// <returns>
        /// A <see cref="IDSSmtpClient" />.
        /// </returns>
        public async Task<IDSSmtpClient> GetClient(Dictionary<string, string> traceProperties)
        {
            if (traceProperties == null)
            {
                traceProperties = new Dictionary<string, string>();
            }

            traceProperties["Id"] = Guid.NewGuid().ToString();

            IDSSmtpClient client = await this.TryGetClient(traceProperties, false).ConfigureAwait(false);
            int numberOfTries = 1;

            while (client == null)
            {
                if (numberOfTries > 5)
                {
                    this.logger.WriteCustomEvent("TryGetClient exceeded max number of tries, overriding client creation.", traceProperties);
                    this.logger.WriteMetric("SmtpClientPool_OneTimeClientCreation", 1, traceProperties);
                    client = await this.TryGetClient(traceProperties, true).ConfigureAwait(false);
                    return client;
                }
                else
                {
                    this.logger.TraceInformation($"ClientPoolRequestFailure, number of tries: {numberOfTries}.", traceProperties);
                    this.logger.WriteMetric("SmtpClientPool_ClientPoolRequestFailure", 1, traceProperties);
                    await Task.Delay(1000 * numberOfTries).ConfigureAwait(false);
                    client = await this.TryGetClient(traceProperties, false).ConfigureAwait(false);
                    numberOfTries++;
                }
            }

            return client;
        }

        /// <summary>
        /// Returns the client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="traceProperties">The trace properties.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ReturnClient(IDSSmtpClient client, Dictionary<string, string> traceProperties)
        {
            if (traceProperties == null)
            {
                traceProperties = new Dictionary<string, string>();
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            traceProperties["Id"] = Guid.NewGuid().ToString();

            await SemaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                if (currentClientCount <= MaxClientCount)
                {
                    ClientList.Enqueue(client);
                    this.logger.TraceInformation("SmtpClient returned to pool.", traceProperties);
                }
                else
                {
                    currentClientCount--;
                    client.Dispose();
                    this.logger.TraceInformation($"SmtpClient disposed. Current count: {currentClientCount}", traceProperties);
                }
            }
            finally
            {
                _ = SemaphoreSlim.Release();
            }
        }

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="traceProperties">The trace properties.</param>
        /// <returns>A <see cref="IDSSmtpClient"/>.</returns>
        internal IDSSmtpClient CreateClient()
        {
            IDSSmtpClient emailClient = this.factory.CreateClient(this.config, this.logger);
            return emailClient;
        }

        private async Task<IDSSmtpClient> TryGetClient(Dictionary<string, string> traceProps, bool overrideMaxCount)
        {
            await SemaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                IDSSmtpClient client;
                if (overrideMaxCount || currentClientCount < MaxClientCount)
                {
                    client = this.CreateClient();
                    if (!overrideMaxCount)
                    {
                        currentClientCount++;
                    }

                    this.logger.TraceInformation($"Requested new SmtpClient, client count: {currentClientCount}.", traceProps);
                    return client;
                }
                else
                {
                    if (ClientList.TryDequeue(out client))
                    {
                        this.logger.TraceInformation("Issuing connected SmptClient.", traceProps);
                    }
                    else
                    {
                        this.logger.TraceInformation("No available SmtpClients, issuing null client.", traceProps);
                    }

                    return client;
                }
            }
            finally
            {
                _ = SemaphoreSlim.Release();
            }
        }
    }
}
