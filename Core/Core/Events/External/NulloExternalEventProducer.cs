namespace Core.Events.External;

/// <summary>
/// A no-operation implementation of the <see cref="IExternalEventProducer"/> interface.
/// This implementation does not forward any events externally.
/// Useful in environments where external publishing is not required.
/// </summary>
public class NulloExternalEventProducer : IExternalEventProducer
{
    /// <summary>
    /// Returns a completed task without performing any external publishing.
    /// </summary>
    /// <param name="event">The event envelope to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An already completed task.</returns>
    public Task PublishAsync(IEventEnvelope @event, CancellationToken ct) => Task.CompletedTask;
}
