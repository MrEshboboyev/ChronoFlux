using Core.Aggregates;
using Core.OptimisticConcurrency;

namespace Core.EventStoreDB.Repository;

/// <summary>
/// Decorates an <see cref="IEventStoreDBRepository{T}"/> instance with ETag (optimistic concurrency) support.
/// It reads and writes expected and next resource versions using the provided version providers.
/// </summary>
/// <typeparam name="T">The aggregate type implementing <see cref="IAggregate"/>.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="EventStoreDBRepositoryWithETagDecorator{T}"/> class.
/// </remarks>
public class EventStoreDBRepositoryWithETagDecorator<T>(
    IEventStoreDBRepository<T> inner,
    IExpectedResourceVersionProvider expectedResourceVersionProvider,
    INextResourceVersionProvider nextResourceVersionProvider) : IEventStoreDBRepository<T>
    where T : class, IAggregate
{

    /// <inheritdoc />
    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        inner.Find(id, cancellationToken);

    /// <inheritdoc />
    public async Task<ulong> Add(Guid id, T aggregate, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Add(id, aggregate, cancellationToken).ConfigureAwait(true);
        // Set the next expected version in the version provider.
        nextResourceVersionProvider.TrySet(nextExpectedVersion.ToString());
        return nextExpectedVersion;
    }

    /// <inheritdoc />
    public async Task<ulong> Update(Guid id, T aggregate, ulong? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Update(id, aggregate, expectedVersion ?? GetExpectedVersion(), cancellationToken).ConfigureAwait(true);
        nextResourceVersionProvider.TrySet(nextExpectedVersion.ToString());
        return nextExpectedVersion;
    }

    /// <inheritdoc />
    public async Task<ulong> Delete(Guid id, T aggregate, ulong? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Delete(id, aggregate, expectedVersion ?? GetExpectedVersion(), cancellationToken).ConfigureAwait(true);
        nextResourceVersionProvider.TrySet(nextExpectedVersion.ToString());
        return nextExpectedVersion;
    }

    /// <summary>
    /// Gets the expected version from the expected resource version provider.
    /// Returns <c>null</c> if the stored value is missing or cannot be parsed.
    /// </summary>
    /// <returns>The expected version or null.</returns>
    private ulong? GetExpectedVersion()
    {
        var value = expectedResourceVersionProvider.Value;
        if (string.IsNullOrWhiteSpace(value) || !ulong.TryParse(value, out var expectedVersion))
            return null;
        return expectedVersion;
    }
}
