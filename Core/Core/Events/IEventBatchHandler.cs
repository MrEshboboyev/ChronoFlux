namespace Core.Events;

/// <summary>
/// Represents a handler responsible for processing a batch of event envelopes.
/// </summary>
public interface IEventBatchHandler
{
    /// <summary>
    /// Handles an array of event envelopes asynchronously.
    /// </summary>
    /// <param name="eventInEnvelopes">Array of event envelopes to process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(IEventEnvelope[] eventInEnvelopes, CancellationToken ct);
}
