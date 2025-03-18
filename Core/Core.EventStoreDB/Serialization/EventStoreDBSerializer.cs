using Core.Events;
using Core.EventStoreDB.Events;
using Core.Serialization.Newtonsoft;
using EventStore.Client;
using Newtonsoft.Json;
using OpenTelemetry.Context.Propagation;
using System.Text;

namespace Core.EventStoreDB.Serialization;

/// <summary>
/// Provides JSON-based serialization and deserialization methods tailored for EventStoreDB.
/// It leverages custom serializer settings that use a non-default constructor contract resolver
/// and a custom converter for event metadata containing propagation context.
/// </summary>
public static class EventStoreDBSerializer
{
    // Custom serializer settings configured once.
    private static readonly JsonSerializerSettings SerializerSettings;

    static EventStoreDBSerializer()
    {
        SerializerSettings = new JsonSerializerSettings()
            .WithNonDefaultConstructorContractResolver();
        // Add custom converter for event metadata (tracing information).
        SerializerSettings.Converters.Add(new EventStoreDBEventMetadataJsonConverter());
    }

    /// <summary>
    /// Deserializes the event data from a ResolvedEvent into an object of the expected type T.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to. Must be a class.</typeparam>
    /// <param name="resolvedEvent">The resolved event containing serialized data.</param>
    /// <returns>An instance of T, or null if deserialization fails.</returns>
    public static T? Deserialize<T>(this ResolvedEvent resolvedEvent) where T : class =>
        Deserialize(resolvedEvent) as T;

    /// <summary>
    /// Deserializes the event data from a ResolvedEvent using the type determined by the event's metadata.
    /// </summary>
    /// <param name="resolvedEvent">The resolved event containing serialized data.</param>
    /// <returns>An object representing the event data, or null if the event type cannot be determined.</returns>
    public static object? Deserialize(this ResolvedEvent resolvedEvent)
    {
        // Determine the event type from the event type name mapped by EventTypeMapper.
        var eventType = EventTypeMapper.Instance.ToType(resolvedEvent.Event.EventType);
        if (eventType == null)
            return null;

        // Deserialize the event payload from UTF8 bytes.
        string jsonData = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
        return JsonConvert.DeserializeObject(jsonData, eventType, SerializerSettings)!;
    }

    /// <summary>
    /// Deserializes the propagation context (tracing information) from the event metadata.
    /// </summary>
    /// <param name="resolvedEvent">The resolved event containing metadata.</param>
    /// <returns>A <see cref="PropagationContext"/> instance if extraction is successful; otherwise, null.</returns>
    public static PropagationContext? DeserializePropagationContext(this ResolvedEvent resolvedEvent)
    {
        // Determine the event type from the event type mapping.
        var eventType = EventTypeMapper.Instance.ToType(resolvedEvent.Event.EventType);
        if (eventType == null)
            return null;

        // Deserialize the metadata from UTF8 bytes.
        string jsonMetadata = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata.Span);
        return JsonConvert.DeserializeObject<PropagationContext>(jsonMetadata, SerializerSettings);
    }

    /// <summary>
    /// Converts an event object and optional metadata into an EventStoreDB <see cref="EventData"/> instance.
    /// </summary>
    /// <param name="event">The event object to serialize.</param>
    /// <param name="metadata">Optional metadata to include with the event.</param>
    /// <returns>An <see cref="EventData"/> instance containing the serialized event and metadata.</returns>
    public static EventData ToJsonEventData(this object @event, object? metadata = null)
    {
        // Serialize event and metadata into JSON bytes.
        var eventJson = JsonConvert.SerializeObject(@event, SerializerSettings);
        var metadataJson = JsonConvert.SerializeObject(metadata ?? new { }, SerializerSettings);
        return new EventData(
            Uuid.NewUuid(),
            EventTypeMapper.Instance.ToName(@event.GetType()),
            Encoding.UTF8.GetBytes(eventJson),
            Encoding.UTF8.GetBytes(metadataJson)
        );
    }
}
