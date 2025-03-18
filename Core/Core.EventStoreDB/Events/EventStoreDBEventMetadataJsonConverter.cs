using Core.OpenTelemetry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTelemetry.Context.Propagation;

namespace Core.EventStoreDB.Events;

/// <summary>
/// A JSON converter for <see cref="PropagationContext"/> objects used in event metadata.
/// This converter serializes important tracing information (such as trace and span IDs)
/// and also injects propagation headers (e.g. traceparent, tracestate).
/// </summary>
public class EventStoreDBEventMetadataJsonConverter : JsonConverter
{
    private const string CorrelationIdPropertyName = "$correlationId";
    private const string CausationIdPropertyName = "$causationId";
    private const string TraceParentPropertyName = "traceparent";
    private const string TraceStatePropertyName = "tracestate";

    /// <summary>
    /// Determines whether this converter can convert the specified object type.
    /// </summary>
    public override bool CanConvert(Type objectType) =>
        objectType == typeof(PropagationContext) || objectType == typeof(PropagationContext?);

    /// <summary>
    /// Writes the JSON representation of a <see cref="PropagationContext"/>, including custom tracing identifiers.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not PropagationContext propagationContext)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();

        // Write correlation and causation identifiers.
        writer.WritePropertyName(CorrelationIdPropertyName);
        writer.WriteValue(propagationContext.ActivityContext.TraceId.ToHexString());

        writer.WritePropertyName(CausationIdPropertyName);
        writer.WriteValue(propagationContext.ActivityContext.SpanId.ToHexString());

        // Inject the rest of the propagation headers.
        propagationContext.Inject(writer, SerializePropagationContext);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Helper method to write a key-value pair for propagation context during serialization.
    /// </summary>
    private static void SerializePropagationContext(JsonWriter writer, string key, string value)
    {
        writer.WritePropertyName(key);
        writer.WriteValue(value);
    }

    /// <summary>
    /// Reads the JSON representation and reconstructs a <see cref="PropagationContext"/> from provided headers.
    /// </summary>
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var headers = new Dictionary<string, string?>
        {
            { TraceParentPropertyName, jObject[TraceParentPropertyName]?.Value<string>() },
            { TraceStatePropertyName, jObject[TraceStatePropertyName]?.Value<string>() }
        };

        return TelemetryPropagator.Extract(headers, ExtractTraceContextFromEventMetadata);
    }

    /// <summary>
    /// Helper function that extracts trace context header values.
    /// </summary>
    private static IEnumerable<string> ExtractTraceContextFromEventMetadata(
        Dictionary<string, string?> headers,
        string key)
    {
        try
        {
            return headers.TryGetValue(key, out var value) && value != null
                ? new[] { value }
                : [];
        }
        catch (Exception ex)
        {
            // Log the exception as needed.
            Console.WriteLine($"Failed to extract trace context: {ex}");
            return [];
        }
    }
}
