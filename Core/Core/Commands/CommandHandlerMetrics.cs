namespace Core.Commands;

/// <summary>
/// Records metrics for command handling operations using OpenTelemetry.
/// Tracks the number of active commands, total commands processed, and handling duration.
/// </summary>
public class CommandHandlerMetrics : IDisposable
{
    private readonly TimeProvider timeProvider;
    private readonly Meter meter;
    private readonly UpDownCounter<long> activeEventHandlingCounter;
    private readonly Counter<long> totalCommandsNumber;
    private readonly Histogram<double> eventHandlingDuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandlerMetrics"/> class.
    /// </summary>
    /// <param name="meterFactory">The factory used to create a Meter instance.</param>
    /// <param name="timeProvider">Provides high-resolution timing for measuring durations.</param>
    public CommandHandlerMetrics(
        IMeterFactory meterFactory,
        TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
        meter = meterFactory.Create(ActivitySourceProvider.DefaultSourceName);

        totalCommandsNumber = meter.CreateCounter<long>(
            TelemetryTags.Commands.TotalCommandsNumber,
            unit: "{command}",
            description: "Total number of commands sent to command handlers");

        activeEventHandlingCounter = meter.CreateUpDownCounter<long>(
            TelemetryTags.Commands.ActiveCommandsNumber,
            unit: "{command}",
            description: "Number of commands currently being handled");

        eventHandlingDuration = meter.CreateHistogram<double>(
            TelemetryTags.Commands.CommandHandlingDuration,
            unit: "s",
            description: "Measures the duration of inbound commands");
    }

    /// <summary>
    /// Marks the start of a command handling operation, updating active and total counts.
    /// </summary>
    /// <param name="commandType">The command type name.</param>
    /// <returns>A timestamp representing the start time.</returns>
    public long CommandHandlingStart(string commandType)
    {
        var tags = new TagList { { TelemetryTags.Commands.CommandType, commandType } };

        if (activeEventHandlingCounter.Enabled)
        {
            activeEventHandlingCounter.Add(1, tags);
        }
        if (totalCommandsNumber.Enabled)
        {
            totalCommandsNumber.Add(1, tags);
        }

        return timeProvider.GetTimestamp();
    }

    /// <summary>
    /// Marks the end of a command handling operation, updating metrics and recording the elapsed time.
    /// </summary>
    /// <param name="commandType">The command type name.</param>
    /// <param name="startingTimestamp">The timestamp returned from <see cref="CommandHandlingStart"/>.</param>
    public void CommandHandlingEnd(string commandType, long startingTimestamp)
    {
        var tags = (activeEventHandlingCounter.Enabled || eventHandlingDuration.Enabled)
            ? new TagList { { TelemetryTags.Commands.CommandType, commandType } }
            : default;

        if (activeEventHandlingCounter.Enabled)
        {
            activeEventHandlingCounter.Add(-1, tags);
        }
        if (!eventHandlingDuration.Enabled) return;

        var elapsed = timeProvider.GetElapsedTime(startingTimestamp);
        eventHandlingDuration.Record(elapsed.TotalSeconds, tags);
    }

    /// <summary>
    /// Disposes the internal Meter instance.
    /// </summary>
    public void Dispose() => meter.Dispose();
}
