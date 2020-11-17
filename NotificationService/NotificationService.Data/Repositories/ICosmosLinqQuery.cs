// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System.Linq;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// Custom cosmos Linq Query.
    /// </summary>
    public interface ICosmosLinqQuery
    {
        /// <summary>
        /// Gets the Feed Iterator for the input query.
        /// </summary>
        /// <typeparam name="T">Type of entity in query.</typeparam>
        /// <param name="query">Query to evaluate.</param>
        /// <returns>Feed Iterator instance.</returns>
        FeedIterator<T> GetFeedIterator<T>(IQueryable<T> query);
    }
}