namespace Core.Events;

/// <summary>
/// Defines a contract for handling events of type TEvent.
/// </summary>
/// <typeparam name="TEvent">The event type to handle.</typeparam>
public interface IEventHandler<in TEvent>
{
    /// <summary>
    /// Processes the given event asynchronously.
    /// </summary>
    /// <param name="event">The event to process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(TEvent @event, CancellationToken ct);
}

/// <summary>
/// A simple implementation of IEventHandler that wraps a delegate function.
/// </summary>
/// <typeparam name="TEvent">The event type to handle.</typeparam>
/// <remarks>
/// Creates a new instance wrapping the specified handler delegate.
/// </remarks>
/// <param name="handler">The delegate to invoke when handling the event.</param>
public class EventHandler<TEvent>(Func<TEvent, CancellationToken, Task> handler) : IEventHandler<TEvent>
{

    /// <summary>
    /// Invokes the handler delegate for the given event.
    /// </summary>
    public Task HandleAsync(TEvent @event, CancellationToken ct) =>
        handler(@event, ct);
}
