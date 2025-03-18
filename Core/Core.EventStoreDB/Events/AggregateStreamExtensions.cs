using Core.Events;
using Core.EventStoreDB.Serialization;
using Core.Projections;
using EventStore.Client;

namespace Core.EventStoreDB.Events;

/// <summary>
/// Provides extension methods for aggregating a stream of domain events into a projection or aggregate.
/// </summary>
public static class AggregateStreamExtensions
{
    /// <summary>
    /// Reads events from an EventStoreDB stream (dedicated to the aggregate identified by the provided id)
    /// and applies them one by one to rehydrate the specified aggregate or projection.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the aggregate or projection implementing <see cref="IProjection"/>.
    /// </typeparam>
    /// <param name="eventStore">The EventStoreDB client instance used to read from the stream.</param>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <param name="fromVersion">
    /// Optional stream version at which to start reading. Defaults to the beginning of the stream.
    /// </param>
    /// <returns>
    /// An instance of the aggregate (if the stream is found) with applied events; otherwise, null.
    /// </returns>
    public static async Task<T?> AggregateStream<T>(
        this EventStoreClient eventStore,
        Guid id,
        CancellationToken cancellationToken,
        ulong? fromVersion = null)
        where T : class, IProjection
    {
        // Build the stream id from the aggregate id.
        string streamId = StreamNameMapper.ToStreamId<T>(id);

        // Read the events from the stream in the forward direction.
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            streamId,
            fromVersion ?? StreamPosition.Start,
            cancellationToken: cancellationToken);

        // If the stream is not found, return null.
        if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            return null;

        // Create an instance of the aggregate. The parameterless constructor is assumed.
        var aggregate = (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;

        // Process each event from the stream and apply it to the aggregate.
        await foreach (var @event in readResult)
        {
            var eventData = @event.Deserialize();
            aggregate.Apply(eventData!);
        }

        return aggregate;
    }
}
