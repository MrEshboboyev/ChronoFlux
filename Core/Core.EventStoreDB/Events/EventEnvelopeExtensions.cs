using Core.Events;
using Core.EventStoreDB.Serialization;
using EventStore.Client;

namespace Core.EventStoreDB.Events;

/// <summary>
/// Provides extension methods to convert EventStoreDB resolved events into domain event envelopes.
/// </summary>
public static class EventEnvelopeExtensions
{
    /// <summary>
    /// Converts a <see cref="ResolvedEvent"/> into an <see cref="IEventEnvelope"/>.
    /// It deserializes both the event data and its associated propagation/metadata.
    /// </summary>
    /// <param name="resolvedEvent">The resolved event from EventStoreDB.</param>
    /// <returns>
    /// An <see cref="IEventEnvelope"/> containing the deserialized event data and metadata,
    /// or null if the event data cannot be deserialized.
    /// </returns>
    public static IEventEnvelope? ToEventEnvelope(this ResolvedEvent resolvedEvent)
    {
        // Deserialize the event payload.
        var eventData = resolvedEvent.Deserialize();
        // Attempt to deserialize propagation context from the event metadata.
        var eventMetadata = resolvedEvent.DeserializePropagationContext();

        if (eventData == null)
            return null;

        // Construct detailed metadata for the event.
        var metaData = new EventMetadata(
            resolvedEvent.Event.EventId.ToString(),
            resolvedEvent.Event.EventNumber.ToUInt64(),
            resolvedEvent.Event.Position.CommitPosition,
            eventMetadata);

        // Create an event envelope using the provided metadata.
        return EventEnvelope.From(eventData, metaData);
    }
}
