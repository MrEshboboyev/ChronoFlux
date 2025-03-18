namespace Core.Events.External;

/// <summary>
/// Defines a contract for consuming external events.
/// Implementers are expected to start and maintain a subscription to an external source.
/// </summary>
public interface IExternalEventConsumer
{
    /// <summary>
    /// Starts the external event consumer.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to signal shutdown.</param>
    /// <returns>A task representing the asynchronous start operation.</returns>
    Task StartAsync(CancellationToken cancellationToken);
}
