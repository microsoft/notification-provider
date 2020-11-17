// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// Query Client Interface for CosmosDB.
    /// </summary>
    public interface ICosmosDBQueryClient : IDisposable
    {
        /// <summary>
        /// Gets an instance of <see cref="Container"/> to perform the query operations.
        /// </summary>
        /// <param name="databaseName">Name of the CosmosDB database.</param>
        /// <param name="containerName">Name of the container within the CosmosDB database.</param>
        /// <returns><see cref="Container"/>.</returns>
        Container GetCosmosContainer(string databaseName, string containerName);
    }
}
