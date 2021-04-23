// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.Data.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using NotificationService.Contracts;
    using NotificationService.Contracts.Entities;

    /// <summary>
    /// The <see cref="IRepository{T}"/> interface provides the mechanism to manage the entity persistence and projection.
    /// </summary>
    /// <typeparam name="T">The specialized type of <see cref="CosmosDBEntity"/>.</typeparam>
    public interface IRepository<T>
        where T : CosmosDBEntity
    {
#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute

#pragma warning disable CS1658 // Warning is overriding an error
        /// <summary>
        /// Upserts the entities asynchronously.
        /// </summary>
        /// <remarks>The repository treats patitionkey = entityId for parameter references.</remarks>
        /// <param name="entities">The entities.</param>
        /// <returns>The instance of <see cref="Task{IEnumerable{T}}"/> representing asynchronous operation with <c>true</c> if successful.</returns>
        Task<IEnumerable<T>> UpsertAsync(IEnumerable<T> entities);
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute

        /// <summary>
        /// Reads the entity asynchronously.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>The instance of <see cref="Task{T}"/>.</returns>
        Task<T> ReadAsync(string entityId);

#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute

#pragma warning disable CS1658 // Warning is overriding an error
        /// <summary>
        /// Reads the entities matching specified filter criteria asynchronously.
        /// </summary>
        /// <param name="filterExpression">The filter expression.</param>
        /// <param name="orderExpression">The order expression.</param>
        /// <param name="nextPageId">The next page identifier.</param>
        /// <param name="numOfEntities">The number of entities to project.</param>
        /// <returns>The instance of <see cref="Task{EntityCollection{T}}"/> representing asynchronous operation.</returns>
        Task<EntityCollection<T>> ReadAsync(Expression<Func<T, bool>> filterExpression, Expression<Func<T, NotificationPriority>> orderExpression, string nextPageId, int numOfEntities = 10);
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute

        /// <summary>
        /// Deletes the entity asynchronously.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>The instance of <see cref="Task{Boolean}"/> representing asynchronous operation with <c>true</c> if successful.</returns>
        Task<bool> DeleteAsync(string entityId);
    }
}
