using Confluent.Kafka;
using Core.Events;
using Core.Events.External;
using Core.Kafka.Events;
using Core.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static Core.Extensions.DictionaryExtensions;

namespace Core.Kafka.Consumers;

/// <summary>
/// Implements an external event consumer that reads messages from Kafka,
/// converts them into event envelopes, and publishes them to the internal event bus.
/// </summary>
public class KafkaConsumer : IExternalEventConsumer
{
    private readonly KafkaConsumerConfig config;
    private readonly IEventBus eventBus;
    private readonly IActivityScope activityScope;
    private readonly ILogger<KafkaConsumer> logger;
    private readonly IConsumer<string, string> consumer;

    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaConsumer"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="eventBus">The internal event bus.</param>
    /// <param name="activityScope">The activity scope for telemetry.</param>
    /// <param name="logger">A logger instance.</param>
    /// <param name="consumer">
    /// Optional Kafka consumer instance. If not provided, a new one is created using the bound configuration.
    /// </param>
    public KafkaConsumer(
        IConfiguration configuration,
        IEventBus eventBus,
        IActivityScope activityScope,
        ILogger<KafkaConsumer> logger,
        IConsumer<string, string>? consumer = null)
    {
        this.eventBus = eventBus;
        this.activityScope = activityScope;
        this.logger = logger;

        // Bind consumer configuration from settings.
        config = configuration.GetKafkaConsumerConfig();
        this.consumer = consumer ?? new ConsumerBuilder<string, string>(config.ConsumerConfig).Build();
    }

    /// <summary>
    /// Starts consuming messages from Kafka.
    /// Subscribes to the configured topics and processes events continuously
    /// until a cancellation is requested.
    /// </summary>
    /// <param name="cancellationToken">A token to signal cancellation.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka consumer started");

        // Subscribe to topics obtained from configuration.
        consumer.Subscribe(config.Topics);

        try
        {
            // Process until cancellation is requested.
            while (!cancellationToken.IsCancellationRequested)
            {
                await ConsumeNextEvent(consumer, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            logger.LogError("Error consuming Kafka message: {Message} {StackTrace}", e.Message, e.StackTrace);
            // Ensure a clean consumer shutdown and offset commit.
            consumer.Close();
        }
    }

    /// <summary>
    /// Consumes the next message from Kafka, converts it into an event envelope,
    /// and publishes it to the internal event bus within an OpenTelemetry activity.
    /// </summary>
    /// <param name="consumer">The Kafka consumer instance.</param>
    /// <param name="token">A cancellation token.</param>
    private async Task ConsumeNextEvent(IConsumer<string, string> consumer, CancellationToken token)
    {
        try
        {
            // Workaround to yield control (remove when the GitHub issue is resolved).
            await Task.Yield();

            // Block until a message is available.
            var message = consumer.Consume(token);

            // Convert the consumed message into a domain event envelope.
            var eventEnvelope = message.ToEventEnvelope();

            if (eventEnvelope == null)
            {
                logger.LogWarning("Couldn't deserialize event of type: {EventType}", message.Message.Key);
                if (!config.IgnoreDeserializationErrors)
                {
                    throw new InvalidOperationException($"Unable to deserialize event {message.Message.Key}");
                }
                return;
            }

            // Wrap publishing in an activity scope for telemetry.
            await activityScope.RunAsync($"{nameof(KafkaConsumer)}/{nameof(ConsumeNextEvent)}",
                async (_, ct) =>
                {
                    await eventBus.PublishAsync(eventEnvelope, ct).ConfigureAwait(false);
                    consumer.Commit();
                },
                new StartActivityOptions
                {
                    // Merge Kafka consumer telemetry tags with additional event information.
                    Tags = Merge(
                        TelemetryTags.Messaging.Kafka.ConsumerTags(
                            config.ConsumerConfig.GroupId,
                            message.Topic,
                            message.Message.Key,
                            message.Partition.Value.ToString(),
                            config.ConsumerConfig.GroupId),
                        new Dictionary<string, object?>
                        {
                            { TelemetryTags.EventHandling.Event, eventEnvelope.Data.GetType() }
                        }),
                    // Pass the propagation context if available.
                    Parent = eventEnvelope.Metadata.PropagationContext?.ActivityContext,
                    Kind = ActivityKind.Consumer
                },
                token
            ).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError("Error consuming Kafka message: {Message} {StackTrace}", e.Message, e.StackTrace);
        }
    }
}
