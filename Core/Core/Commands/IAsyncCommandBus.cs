namespace Core.Commands;

/// <summary>
/// Represents a command bus that supports asynchronous command scheduling.
/// </summary>
public interface IAsyncCommandBus
{
    /// <summary>
    /// Schedules a command to be executed asynchronously.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to schedule.</typeparam>
    /// <param name="command">The command instance.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous scheduling operation.</returns>
    Task ScheduleAsync<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : notnull;
}
