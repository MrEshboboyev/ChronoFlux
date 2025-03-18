using Core.Aggregates;
using Core.Events;
using Core.EventStoreDB.Events;
using Core.EventStoreDB.Serialization;
using Core.OpenTelemetry;
using EventStore.Client;

namespace Core.EventStoreDB.Repository;

/// <summary>
/// Defines the contract for an EventStoreDB based repository that can perform CRUD operations
/// on an aggregate.
/// </summary>
/// <typeparam name="T">
/// The aggregate type that implements <see cref="IAggregate"/>.
/// </typeparam>
public interface IEventStoreDBRepository<T> where T : class, IAggregate
{
    /// <summary>
    /// Finds an aggregate by its identifier.
    /// </summary>
    Task<T?> Find(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an aggregate to the store.
    /// </summary>
    Task<ulong> Add(Guid id, T aggregate, CancellationToken ct = default);

    /// <summary>
    /// Updates an aggregate in the store, optionally expecting a specific revision.
    /// </summary>
    Task<ulong> Update(Guid id, T aggregate, ulong? expectedRevision = null, CancellationToken ct = default);

    /// <summary>
    /// Deletes an aggregate from the store by updating it (using the same update semantics).
    /// </summary>
    Task<ulong> Delete(Guid id, T aggregate, ulong? expectedRevision = null, CancellationToken ct = default);
}

/// <summary>
/// Implements an EventStoreDB based repository which reads event streams to rehydrate an aggregate
/// and appends events to the stream.
/// </summary>
/// <typeparam name="T">
/// The aggregate type that implements <see cref="IAggregate"/>.
/// </typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="EventStoreDBRepository{T}"/> class.
/// </remarks>
/// <param name="eventStore">The EventStoreDB client instance.</param>
public class EventStoreDBRepository<T>(EventStoreClient eventStore) : IEventStoreDBRepository<T>
    where T : class, IAggregate
{

    /// <inheritdoc />
    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        eventStore.AggregateStream<T>(id, cancellationToken);

    /// <inheritdoc />
    public async Task<ulong> Add(Guid id, T aggregate, CancellationToken ct = default)
    {
        var result = await eventStore.AppendToStreamAsync(
            StreamNameMapper.ToStreamId<T>(id),
            StreamState.NoStream,
            GetEventsToStore(aggregate),
            cancellationToken: ct)
            .ConfigureAwait(false);

        return result.NextExpectedStreamRevision.ToUInt64();
    }

    /// <inheritdoc />
    public async Task<ulong> Update(Guid id, T aggregate, ulong? expectedRevision = null, CancellationToken ct = default)
    {
        var eventsToAppend = GetEventsToStore(aggregate);
        // Determine the starting expected stream revision based on aggregate version and event count.
        var nextVersion = expectedRevision ?? (ulong)(aggregate.Version - eventsToAppend.Count);

        var result = await eventStore.AppendToStreamAsync(
            StreamNameMapper.ToStreamId<T>(id),
            nextVersion,
            eventsToAppend,
            cancellationToken: ct)
            .ConfigureAwait(false);

        return result.NextExpectedStreamRevision.ToUInt64();
    }

    /// <inheritdoc />
    public Task<ulong> Delete(Guid id, T aggregate, ulong? expectedRevision = null, CancellationToken ct = default) =>
        Update(id, aggregate, expectedRevision, ct);

    /// <summary>
    /// Extracts and converts uncommitted events from the aggregate into <see cref="EventData"/> objects.
    /// </summary>
    /// <param name="aggregate">The aggregate instance.</param>
    /// <returns>A list of <see cref="EventData"/> ready to be written to the stream.</returns>
    private static List<EventData> GetEventsToStore(T aggregate)
    {
        var events = aggregate.DequeueUncommittedEvents();
        // Convert each event into JSON-serialized EventData including propagation information.
        return [.. events.Select(@event => @event.ToJsonEventData(TelemetryPropagator.GetPropagationContext()))];
    }
}
