// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data
{
    using System.Linq;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;

    /// <summary>
    /// Custom Cosmos Linq Query.
    /// </summary>
    public class CustomCosmosLinqQuery : ICosmosLinqQuery
    {
        /// <inheritdoc/>
        public FeedIterator<T> GetFeedIterator<T>(IQueryable<T> query)
        {
            return query.ToFeedIterator();
        }
    }
}
