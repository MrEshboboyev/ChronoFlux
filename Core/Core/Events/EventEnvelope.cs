namespace Core.Events;

/// <summary>
/// Contains metadata associated with an event.
/// </summary>
/// <param name="EventId">Unique event identifier.</param>
/// <param name="StreamPosition">Position within the event stream.</param>
/// <param name="LogPosition">Position within the event log.</param>
/// <param name="PropagationContext">Optional propagation context for distributed tracing.</param>
public record EventMetadata(
    string EventId,
    ulong StreamPosition,
    ulong LogPosition,
    PropagationContext? PropagationContext
);

/// <summary>
/// Represents a generic envelope that wraps event data along with its metadata.
/// </summary>
public interface IEventEnvelope
{
    /// <summary>
    /// The encapsulated event data.
    /// </summary>
    object Data { get; }

    /// <summary>
    /// Metadata associated with the event.
    /// </summary>
    EventMetadata Metadata { get; init; }
}

/// <summary>
/// Generic implementation of IEventEnvelope.
/// </summary>
/// <typeparam name="T">The type of event data.</typeparam>
public record EventEnvelope<T>(
    T Data,
    EventMetadata Metadata
) : IEventEnvelope where T : notnull
{
    object IEventEnvelope.Data => Data;
}

/// <summary>
/// Provides helper methods to create event envelopes.
/// </summary>
public static class EventEnvelope
{
    /// <summary>
    /// Creates an IEventEnvelope from a raw event data and given metadata using reflection.
    /// Note: Reflection is used here (TODO: consider removing to improve performance).
    /// </summary>
    /// <param name="data">Raw event data.</param>
    /// <param name="metadata">Associated event metadata.</param>
    /// <returns>An instance of IEventEnvelope.</returns>
    public static IEventEnvelope From(object data, EventMetadata metadata)
    {
        var type = typeof(EventEnvelope<>).MakeGenericType(data.GetType());
        return (IEventEnvelope)Activator.CreateInstance(type, data, metadata)!;
    }

    /// <summary>
    /// Creates a new EventEnvelope with default metadata for the given event data.
    /// </summary>
    /// <typeparam name="T">The type of event.</typeparam>
    /// <param name="data">The event data.</param>
    /// <returns>A new EventEnvelope.</returns>
    public static EventEnvelope<T> From<T>(T data) where T : notnull =>
        new(data, new EventMetadata(Guid.NewGuid().ToString(), 0, 0, null));
}
