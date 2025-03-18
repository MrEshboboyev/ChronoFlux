namespace Core.Events.External;

/// <summary>
/// Defines a contract for publishing events to an external system.
/// This can be used to, for example, publish events to a message broker or an API.
/// </summary>
public interface IExternalEventProducer
{
    /// <summary>
    /// Publishes an event envelope to an external endpoint asynchronously.
    /// </summary>
    /// <param name="event">The event envelope to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync(IEventEnvelope @event, CancellationToken ct);
}

/// <summary>
/// Decorates an internal event bus with external publishing capability.
/// After publishing an event internally, if the event data implements <see cref="IExternalEvent"/>,
/// the event is also forwarded to an external producer.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EventBusDecoratorWithExternalProducer"/> class.
/// </remarks>
/// <param name="eventBus">The internal event bus to decorate.</param>
/// <param name="externalEventProducer">The external event producer instance.</param>
public class EventBusDecoratorWithExternalProducer(
    IEventBus eventBus,
    IExternalEventProducer externalEventProducer) : IEventBus
{

    /// <summary>
    /// Publishes the event internally and, if the event data implements <see cref="IExternalEvent"/>,
    /// also publishes it externally.
    /// </summary>
    /// <param name="eventEnvelope">The event envelope to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PublishAsync(IEventEnvelope eventEnvelope, CancellationToken ct)
    {
        // Publish event using the internal event bus.
        await eventBus.PublishAsync(eventEnvelope, ct).ConfigureAwait(false);

        // If the event data indicates it should be published externally, do so.
        if (eventEnvelope.Data is IExternalEvent)
        {
            await externalEventProducer.PublishAsync(eventEnvelope, ct).ConfigureAwait(false);
        }
    }
}
