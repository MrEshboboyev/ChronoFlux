using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.OpenTelemetry.Serialization;

/// <summary>
/// A JSON converter for <see cref="PropagationContext"/> objects.
/// This converter serializes and deserializes the trace context by exposing
/// standard properties such as "traceparent" and "tracestate".
/// </summary>
public class PropagationContextJsonConverter : JsonConverter
{
    private const string TraceParentPropertyName = "traceparent";
    private const string TraceStatePropertyName = "tracestate";

    /// <summary>
    /// Determines if the converter can handle the specified object type.
    /// </summary>
    /// <param name="objectType">The type to be converted.</param>
    /// <returns>True if the type is <see cref="PropagationContext"/> or nullable; otherwise, false.</returns>
    public override bool CanConvert(Type objectType) =>
        objectType == typeof(PropagationContext) || objectType == typeof(PropagationContext?);

    /// <summary>
    /// Serializes the given <see cref="PropagationContext"/> as JSON.
    /// The method injects the "traceparent" and "tracestate" values into the output.
    /// </summary>
    /// <param name="writer">The writer to write JSON to.</param>
    /// <param name="value">The object value to convert.</param>
    /// <param name="serializer">The calling serializer instance.</param>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not PropagationContext propagationContext)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        // The context is injected using a custom delegate that writes properties to the writer.
        propagationContext.Inject(writer, SerializePropagationContext);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Helper method invoked during JSON serialization to write a key-value pair.
    /// </summary>
    /// <param name="writer">The writer to write the property.</param>
    /// <param name="key">The header key (e.g., "traceparent").</param>
    /// <param name="value">The header value.</param>
    private static void SerializePropagationContext(JsonWriter writer, string key, string value)
    {
        writer.WritePropertyName(key);
        writer.WriteValue(value);
    }

    /// <summary>
    /// Deserializes a <see cref="PropagationContext"/> from JSON.
    /// Expects to find "traceparent" and optionally "tracestate" in the JSON object,
    /// which are used to reconstruct the propagation context.
    /// </summary>
    /// <param name="reader">The reader to read JSON from.</param>
    /// <param name="objectType">The target object type.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="serializer">The calling serializer instance.</param>
    /// <returns>The deserialized <see cref="PropagationContext"/>.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        // Load the JSON object from the reader.
        var jObject = JObject.Load(reader);

        // Build a dictionary for the expected headers.
        var headers = new Dictionary<string, string?>
        {
            { TraceParentPropertyName, jObject[TraceParentPropertyName]?.Value<string>() },
            { TraceStatePropertyName, jObject[TraceStatePropertyName]?.Value<string>() }
        };

        // Use the TelemetryPropagator to extract the propagation context.
        return TelemetryPropagator.Extract(headers, ExtractTraceContextFromEventMetadata);
    }

    /// <summary>
    /// Helper function to extract a header value from the dictionary.
    /// Returns a one-element enumerable if found or an empty array if not.
    /// </summary>
    /// <param name="headers">The dictionary containing header values.</param>
    /// <param name="key">The header key to retrieve.</param>
    /// <returns>An enumerable containing the header value, if any.</returns>
    private static IEnumerable<string> ExtractTraceContextFromEventMetadata(Dictionary<string, string?> headers, string key) =>
        headers.TryGetValue(key, out var value) && value != null
            ? [value]
            : Array.Empty<string>();
}
