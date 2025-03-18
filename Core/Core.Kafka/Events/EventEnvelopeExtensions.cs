using Confluent.Kafka;
using Core.Events;
using Core.OpenTelemetry.Serialization;
using Core.Reflection;
using Core.Serialization.Newtonsoft;

namespace Core.Kafka.Events;

/// <summary>
/// Provides extension methods to convert Kafka messages into event envelopes.
/// </summary>
public static class EventEnvelopeExtensions
{
    /// <summary>
    /// Converts a Kafka <see cref="ConsumeResult{TKey, TValue}"/> message to an <see cref="IEventEnvelope"/>.
    /// Uses the message key to determine the event type.
    /// </summary>
    /// <param name="message">The consumed Kafka message.</param>
    /// <returns>An <see cref="IEventEnvelope"/> representing the domain event, or null if the type cannot be determined.</returns>
    public static IEventEnvelope? ToEventEnvelope(this ConsumeResult<string, string> message)
    {
        // Retrieve the event type using the message key.
        var eventType = TypeProvider.GetTypeFromAnyReferencingAssembly(message.Message.Key);
        if (eventType == null)
            return null;

        // Construct the appropriate generic EventEnvelope type.
        var eventEnvelopeType = typeof(EventEnvelope<>).MakeGenericType(eventType);

        // Deserialize the JSON value into an EventEnvelope instance.
        return message.Message.Value.FromJson(eventEnvelopeType, new PropagationContextJsonConverter()) as IEventEnvelope;
    }
}
