namespace Core.Events;

/// <summary>
/// Handles a batch of event envelopes by publishing each event via the EventBus.
/// This handler wraps publishing in an activity scope to capture telemetry.
/// </summary>
/// <remarks>
/// Constructs a new instance of the EventBusBatchHandler.
/// </remarks>
/// <param name="eventBus">The event bus used for publishing events.</param>
/// <param name="activityScope">The activity scope for telemetry.</param>
/// <param name="logger">A logger instance.</param>
public class EventBusBatchHandler(
    IEventBus eventBus,
    IActivityScope activityScope,
    ILogger<EventBusBatchHandler> logger) : IEventBatchHandler
{

    /// <summary>
    /// Processes an array of event envelopes sequentially.
    /// </summary>
    /// <param name="eventInEnvelopes">Array of event envelopes.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(IEventEnvelope[] eventInEnvelopes, CancellationToken ct)
    {
        foreach (var @event in eventInEnvelopes)
        {
            await HandleEventAsync(@event, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles a single event envelope by publishing it via the event bus,
    /// wrapped inside an activity scope to capture telemetry details.
    /// </summary>
    /// <param name="eventEnvelope">The event envelope to handle.</param>
    /// <param name="token">Cancellation token.</param>
    private async Task HandleEventAsync(IEventEnvelope eventEnvelope, CancellationToken token)
    {
        try
        {
            await activityScope.RunAsync(
                $"{nameof(EventBusBatchHandler)}/{nameof(HandleEventAsync)}",
                async (_, ct) =>
                {
                    // Publish the event via the internal event bus.
                    await eventBus.PublishAsync(eventEnvelope, ct).ConfigureAwait(false);
                },
                new StartActivityOptions
                {
                    Tags = { { TelemetryTags.EventHandling.Event, eventEnvelope.Data.GetType().Name } },
                    Parent = eventEnvelope.Metadata.PropagationContext?.ActivityContext,
                    Kind = ActivityKind.Consumer
                },
                token
            ).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError("Error consuming message: {ExceptionMessage} {ExceptionStackTrace}", e.Message, e.StackTrace);
            // Optionally add error processing logic here before rethrowing.
            throw;
        }
    }
}
