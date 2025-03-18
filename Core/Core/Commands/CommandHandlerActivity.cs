namespace Core.Commands;

/// <summary>
/// Wraps command execution within an OpenTelemetry activity,
/// using metrics to track command handling duration.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CommandHandlerActivity"/> class.
/// </remarks>
/// <param name="metrics">The metrics instance used for recording command handling information.</param>
public class CommandHandlerActivity(CommandHandlerMetrics metrics)
{
    /// <summary>
    /// Attempts to execute a command handler within an activity and records its execution time.
    /// </summary>
    /// <typeparam name="TCommand">The type of command being processed.</typeparam>
    /// <param name="activityScope">The activity scope for telemetry.</param>
    /// <param name="commandHandlerName">The name of the command handler.</param>
    /// <param name="run">A delegate representing the command handling operation.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// A task that returns true upon successful execution of the command handler.
    /// </returns>
    public async Task<bool> TrySendAsync<TCommand>(
        IActivityScope activityScope,
        string commandHandlerName,
        Func<Activity?, CancellationToken, Task> run,
        CancellationToken ct)
    {
        var commandName = typeof(TCommand).Name;
        var activityName = $"{commandHandlerName}/{commandName}";

        // Record the start time and update metrics for the command.
        var startingTimestamp = metrics.CommandHandlingStart(commandName);

        try
        {
            await activityScope.RunAsync(
                activityName,
                run,
                new StartActivityOptions
                {
                    Tags = { { TelemetryTags.Commands.Command, commandName } },
                    Kind = ActivityKind.Consumer
                },
                ct
            ).ConfigureAwait(false);
        }
        finally
        {
            // Record the command handling end time and elapsed duration.
            metrics.CommandHandlingEnd(commandName, startingTimestamp);
        }

        return true;
    }
}
