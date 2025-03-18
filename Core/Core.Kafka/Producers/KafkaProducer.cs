using Confluent.Kafka;
using Core.Events;
using Core.Events.External;
using Core.OpenTelemetry;
using Core.OpenTelemetry.Serialization;
using Core.Serialization.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static Core.Extensions.DictionaryExtensions;

namespace Core.Kafka.Producers;

/// <summary>
/// Implements an external event producer that publishes event envelopes to Kafka.
/// The producer utilizes OpenTelemetry to capture production telemetry.
/// </summary>
public class KafkaProducer : IExternalEventProducer
{
    private readonly KafkaProducerConfig config;
    private readonly IActivityScope activityScope;
    private readonly ILogger<KafkaProducer> logger;
    private readonly IProducer<string, string> producer;

    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaProducer"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="activityScope">The activity scope for telemetry instrumentation.</param>
    /// <param name="logger">A logger for KafkaProducer.</param>
    /// <param name="producer">
    /// An optional producer instance; if not provided, one is built based on the configuration.
    /// </param>
    public KafkaProducer(
        IConfiguration configuration,
        IActivityScope activityScope,
        ILogger<KafkaProducer> logger,
        IProducer<string, string>? producer = null)
    {
        this.activityScope = activityScope;
        this.logger = logger;
        config = configuration.GetKafkaProducerConfig();
        this.producer = producer ?? new ProducerBuilder<string, string>(config.ProducerConfig).Build();
    }

    /// <summary>
    /// Publishes an event envelope to Kafka. The event envelope is JSON-serialized,
    /// and production is wrapped in an activity to capture telemetry.
    /// </summary>
    /// <param name="event">The event envelope to publish.</param>
    /// <param name="token">A cancellation token.</param>
    public async Task PublishAsync(IEventEnvelope @event, CancellationToken token)
    {
        try
        {
            await activityScope.RunAsync($"{nameof(KafkaProducer)}/{nameof(PublishAsync)}",
                async (_, ct) =>
                {
                    // Create a short cancellation token for production.
                    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(config.ProducerTimeoutInMs ?? 10000));

                    // Produce the Kafka message: include event type in Key and JSON-serialized envelope as Value.
                    await producer.ProduceAsync(config.Topic,
                        new Message<string, string>
                        {
                            Key = @event.Data.GetType().Name,
                            Value = @event.ToJson(new PropagationContextJsonConverter())
                        },
                        cts.Token).ConfigureAwait(false);
                },
                new StartActivityOptions
                {
                    // Merge Kafka producer telemetry tags with additional event details.
                    Tags = Merge(
                        TelemetryTags.Messaging.Kafka.ProducerTags(
                            config.Topic,
                            config.Topic,
                            @event.Data.GetType().Name),
                        new Dictionary<string, object?>
                        {
                            { TelemetryTags.EventHandling.Event, @event.Data.GetType() }
                        }),
                    Kind = ActivityKind.Producer
                },
                token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError("Error producing Kafka message: {Message} {StackTrace}", e.Message, e.StackTrace);
            throw;
        }
    }
}
