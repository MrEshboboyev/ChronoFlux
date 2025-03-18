using Core.EventStoreDB.Serialization;
using Core.Exceptions;
using Core.OpenTelemetry;
using Core.Reflection;
using EventStore.Client;

namespace Core.EventStoreDB.Events;

/// <summary>
/// Provides extension methods for the <see cref="EventStoreClient"/> to work with
/// domain events, including reading, appending, and rehydrating aggregates.
/// </summary>
public static class EventStoreDBExtensions
{
    /// <summary>
    /// Finds and aggregates a stream into an instance of <typeparamref name="TEntity"/> by reading events from the stream.
    /// If the stream is not found, returns null.
    /// </summary>
    /// <typeparam name="TEntity">The aggregate or projection type to rehydrate.</typeparam>
    /// <param name="eventStore">The EventStoreDB client.</param>
    /// <param name="when">
    /// A function that takes the current entity and an event, then produces a new entity state.
    /// </param>
    /// <param name="id">The stream identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// The rehydrated entity or null if the stream is not found.
    /// </returns>
    public static async Task<TEntity?> Find<TEntity>(
        this EventStoreClient eventStore,
        Func<TEntity, object, TEntity> when,
        string id,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        // Read the stream from the beginning.
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            id,
            StreamPosition.Start,
            cancellationToken: cancellationToken);

        // If the stream is not found, return null.
        if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            return null;

        // Use the aggregate operator over the event stream.
        // Force non-null by using the registered default/uninitialized object factory.
        return await readResult
            .Select(@event => @event.Deserialize()!)
            .AggregateAsync(
                ObjectFactory<TEntity>.GetDefaultOrUninitialized(),
                when,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Finds and aggregates a stream into an instance of <typeparamref name="TEntity"/>.
    /// If the stream is not found, throws an <see cref="AggregateNotFoundException"/>.
    /// </summary>
    /// <typeparam name="TEntity">The aggregate or projection type.</typeparam>
    /// <param name="eventStore">The EventStoreDB client.</param>
    /// <param name="getDefault">A function to create the default instance.</param>
    /// <param name="when">
    /// A function that applies an event on the current entity, producing the new state.
    /// </param>
    /// <param name="id">The stream identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The fully aggregated entity.</returns>
    /// <exception cref="AggregateNotFoundException">Thrown when the stream is not found.</exception>
    public static async Task<TEntity> Find<TEntity>(
        this EventStoreClient eventStore,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        string id,
        CancellationToken cancellationToken)
    {
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            id,
            StreamPosition.Start,
            cancellationToken: cancellationToken);

        if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            throw AggregateNotFoundException.For<TEntity>(id);

        return await readResult
            .Select(@event => @event.Deserialize()!)
            .AggregateAsync(
                getDefault(),
                when,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Reads all events from the specified stream and returns them as a list of objects.
    /// If the stream is not found, returns an empty list.
    /// </summary>
    /// <param name="eventStore">The EventStoreDB client.</param>
    /// <param name="id">The stream identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of deserialized event objects.</returns>
    public static async Task<List<object>> ReadStream(
        this EventStoreClient eventStore,
        string id,
        CancellationToken cancellationToken)
    {
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            id,
            StreamPosition.Start,
            cancellationToken: cancellationToken);

        if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            return new List<object>();

        return await readResult
            .Select(@event => @event.Deserialize()!)
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Appends a single event object to a stream and returns the next expected stream revision.
    /// It uses a "no stream" option for optimistic writes.
    /// </summary>
    /// <param name="eventStore">The EventStoreDB client.</param>
    /// <param name="id">The stream identifier.</param>
    /// <param name="event">The event to append.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The next expected stream revision.</returns>
    public static async Task<ulong> Append(
        this EventStoreClient eventStore,
        string id,
        object @event,
        CancellationToken cancellationToken)
    {
        // Create event data, injecting propagation context if available.
        var result = await eventStore.AppendToStreamAsync(
            id,
            StreamState.NoStream,
            new[] { @event.ToJsonEventData(TelemetryPropagator.GetPropagationContext()) },
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result.NextExpectedStreamRevision;
    }

    /// <summary>
    /// Appends an event to a stream using a specified expected revision.
    /// </summary>
    /// <param name="eventStore">The EventStoreDB client.</param>
    /// <param name="id">The stream identifier.</param>
    /// <param name="event">The event to append.</param>
    /// <param name="expectedRevision">The expected stream revision.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The next expected stream revision.</returns>
    public static async Task<ulong> Append(
        this EventStoreClient eventStore,
        string id,
        object @event,
        ulong expectedRevision,
        CancellationToken cancellationToken)
    {
        var result = await eventStore.AppendToStreamAsync(
            id,
            expectedRevision,
            new[] { @event.ToJsonEventData(TelemetryPropagator.GetPropagationContext()) },
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result.NextExpectedStreamRevision;
    }

    /// <summary>
    /// Reads the last event from the stream and deserializes it as <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">The expected event type.</typeparam>
    /// <param name="eventStore">The EventStoreDB client.</param>
    /// <param name="id">The stream identifier.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The deserialized last event, or null if not available.</returns>
    public static async Task<TEvent?> ReadLastEvent<TEvent>(
        this EventStoreClient eventStore,
        string id,
        CancellationToken ct)
        where TEvent : class
    {
        var resolvedEvent = await eventStore.ReadLastEvent(id, ct).ConfigureAwait(false);
        return resolvedEvent?.Deserialize<TEvent>();
    }

    /// <summary>
    /// Reads the last event (as a <see cref="ResolvedEvent"/>) from the stream.
    /// Returns <c>null</c> if the stream does not exist.
    /// </summary>
    /// <param name="eventStore">The EventStoreDB client.</param>
    /// <param name="id">The stream identifier.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The last resolved event, or null if not found.</returns>
    public static async Task<ResolvedEvent?> ReadLastEvent(
        this EventStoreClient eventStore,
        string id,
        CancellationToken ct)
    {
        var result = eventStore.ReadStreamAsync(
            Direction.Backwards,
            id,
            StreamPosition.End,
            maxCount: 1,
            cancellationToken: ct);

        if (await result.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            return null;

        return await result.FirstAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Attempts to append a single event to an existing stream. If the append fails,
    /// it updates the stream metadata (limiting events to 1) and then tries to append again.
    /// </summary>
    /// <param name="eventStore">The EventStoreDB client.</param>
    /// <param name="id">The stream identifier.</param>
    /// <param name="event">The event to append.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the append operation.</returns>
    public static async Task AppendToStreamWithSingleEvent(
        this EventStoreClient eventStore,
        string id,
        object @event,
        CancellationToken ct)
    {
        var eventData = new[] { @event.ToJsonEventData() };

        // First attempt: try appending assuming the stream already exists.
        var result = await eventStore.AppendToStreamAsync(
            id,
            StreamState.StreamExists,
            eventData,
            options => { options.ThrowOnAppendFailure = false; },
            cancellationToken: ct)
            .ConfigureAwait(false);

        // If the append succeeds, simply return.
        if (result is SuccessResult)
            return;

        // Otherwise, update stream metadata to enforce a maxCount of one.
        await eventStore.SetStreamMetadataAsync(
            id,
            StreamState.NoStream,
            new StreamMetadata(maxCount: 1),
            cancellationToken: ct)
            .ConfigureAwait(false);

        // And try to append again with stream state set to NoStream.
        await eventStore.AppendToStreamAsync(
            id,
            StreamState.NoStream,
            eventData,
            cancellationToken: ct)
            .ConfigureAwait(false);
    }
}
