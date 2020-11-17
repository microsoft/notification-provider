// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Contracts.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// The <see cref="EntityCollection{T}"/> class stores the items and the continuation page token.
    /// </summary>
    /// <typeparam name="T">The instance for <see cref="CosmosDBEntity"/>.</typeparam>
    public class EntityCollection<T>
        where T : CosmosDBEntity
    {
        /// <summary>
        /// Gets or sets the entity items.
        /// </summary>
        /// <value>
        /// The entitiy items.
        /// </value>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Gets or sets the next page identifier.
        /// </summary>
        /// <value>
        /// The next page identifier.
        /// </value>
        public string NextPageId { get; set; }
    }
}
