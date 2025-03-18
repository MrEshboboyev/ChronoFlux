using Core.Aggregates;
using Core.Exceptions;

namespace Core.EventStoreDB.Repository;

/// <summary>
/// Provides extension methods for working with <see cref="IEventStoreDBRepository{T}"/>
/// instances, such as retrieving an aggregate or updating it in a fluent manner.
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Retrieves an aggregate by its identifier. Throws an <see cref="AggregateNotFoundException"/>
    /// if the aggregate is not found.
    /// </summary>
    /// <typeparam name="T">The aggregate type.</typeparam>
    /// <param name="repository">The repository instance.</param>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The aggregate instance.</returns>
    /// <exception cref="AggregateNotFoundException">If no aggregate is found.</exception>
    public static async Task<T> Get<T>(
        this IEventStoreDBRepository<T> repository,
        Guid id,
        CancellationToken ct)
        where T : class, IAggregate
    {
        var entity = await repository.Find(id, ct).ConfigureAwait(false);
        return entity ?? throw AggregateNotFoundException.For<T>(id);
    }

    /// <summary>
    /// Retrieves an aggregate, applies a given action to modify it, and then updates it in the store.
    /// </summary>
    /// <typeparam name="T">The aggregate type.</typeparam>
    /// <param name="repository">The repository instance.</param>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="action">An action that mutates the aggregate.</param>
    /// <param name="expectedVersion">
    /// An optional expected version to enforce optimistic concurrency.
    /// </param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The next expected stream revision after the update.</returns>
    public static async Task<ulong> GetAndUpdate<T>(
        this IEventStoreDBRepository<T> repository,
        Guid id,
        Action<T> action,
        ulong? expectedVersion = null,
        CancellationToken ct = default)
        where T : class, IAggregate
    {
        var entity = await repository.Get(id, ct).ConfigureAwait(false);
        action(entity);
        return await repository.Update(id, entity, expectedVersion, ct).ConfigureAwait(false);
    }
}
