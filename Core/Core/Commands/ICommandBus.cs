namespace Core.Commands;

/// <summary>
/// Defines an interface for sending commands to registered command handlers.
/// </summary>
public interface ICommandBus
{
    /// <summary>
    /// Sends a command by finding and invoking its registered handler.
    /// Throws an exception if no handler is found.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="command">The command to send.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a command handler is not found.
    /// </exception>
    Task SendAsync<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : notnull;

    /// <summary>
    /// Attempts to send a command by invoking its registered handler.
    /// Returns false if no handler is found.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="command">The command to attempt sending.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// A task that returns true if the command was handled; otherwise, false.
    /// </returns>
    Task<bool> TrySendAsync<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : notnull;
}
