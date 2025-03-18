namespace Core.Commands;

/// <summary>
/// Defines a handler for processing commands of a specific type.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
public interface ICommandHandler<in TCommand>
{
    /// <summary>
    /// Processes the given command asynchronously.
    /// </summary>
    /// <param name="command">The command to process.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(TCommand command, CancellationToken ct);
}
