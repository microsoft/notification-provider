// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Options;
    using NotificationService.Common;

    /// <summary>
    /// Query Client to interact with CosmosDB.
    /// </summary>
    public class CosmosDBQueryClient : ICosmosDBQueryClient
    {
        /// <summary>
        /// Instance of <see cref="CosmosDBSetting"/>.
        /// </summary>
        private readonly CosmosDBSetting cosmosDBSetting;

        /// <summary>
        /// Instance of <see cref="CosmosClient"/>.
        /// </summary>
        private readonly CosmosClient cosmosClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBQueryClient"/> class.
        /// </summary>
        /// <param name="cosmosDBSetting">Cosmos DB configuration.</param>
        public CosmosDBQueryClient(IOptions<CosmosDBSetting> cosmosDBSetting)
        {
            this.cosmosDBSetting = cosmosDBSetting?.Value;
            this.cosmosClient = new CosmosClient(this.cosmosDBSetting.Uri, this.cosmosDBSetting.Key);
        }

        /// <inheritdoc/>
        public Container GetCosmosContainer(string databaseName, string containerName)
        {
            return this.cosmosClient.GetContainer(databaseName, containerName);
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
            if (disposing)
            {
                this.cosmosClient?.Dispose();
            }
        }
    }
}
