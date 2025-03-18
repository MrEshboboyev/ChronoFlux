using Core.Events;
using Quartz;
using static Core.Events.TimeHasPassed; // For MinuteHasPassed, HourHasPassed, DayHasPassed

namespace Core.Scheduling;

/// <summary>
/// A Quartz job that publishes a "passage of time" event.
/// This job retrieves a time unit from its job data, converts it into a time event,
/// and publishes the event via the internal event bus.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PassageOfTimeJob"/> class.
/// </remarks>
/// <param name="eventBus">The event bus used to publish events.</param>
/// <param name="timeProvider">A provider for the current UTC time.</param>
public class PassageOfTimeJob(IEventBus eventBus, TimeProvider timeProvider) : IJob
{

    /// <summary>
    /// Executes the job by creating a time-passage event and publishing it.
    /// </summary>
    /// <param name="context">The Quartz job execution context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task Execute(IJobExecutionContext context)
    {
        // Retrieve the time unit string and convert it to a TimeUnit enum.
        var timeUnit = context.MergedJobDataMap.GetString("timeUnit")!.ToTimeUnit();

        // Create the time event using the current UTC time and the previous fire time.
        var timeEvent = timeUnit.ToEvent(timeProvider.GetUtcNow(), context.PreviousFireTimeUtc);

        // Publish the event wrapped in an envelope.
        return eventBus.PublishAsync(EventEnvelope.From(timeEvent), CancellationToken.None);
    }
}

/// <summary>
/// Represents units of time used for scheduling passage of time events.
/// </summary>
public enum TimeUnit
{
    Minute,
    Hour,
    Day
}

/// <summary>
/// Provides extension methods for converting strings and enums to TimeUnit values
/// and creating time passage events.
/// </summary>
public static class TimeUnitExtensions
{
    /// <summary>
    /// Converts a string representation of a time unit to its corresponding <see cref="TimeUnit"/>.
    /// </summary>
    /// <param name="timeUnitString">The string representation (e.g., "Minute").</param>
    /// <returns>The parsed <see cref="TimeUnit"/> value.</returns>
    public static TimeUnit ToTimeUnit(this string timeUnitString) =>
        Enum.Parse<TimeUnit>(timeUnitString);

    /// <summary>
    /// Converts a <see cref="TimeUnit"/> into its equivalent <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="timeUnit">The time unit.</param>
    /// <returns>A <see cref="TimeSpan"/> corresponding to the time unit.</returns>
    public static TimeSpan ToTimeSpan(this TimeUnit timeUnit) =>
        timeUnit switch
        {
            TimeUnit.Minute => TimeSpan.FromMinutes(1),
            TimeUnit.Hour => TimeSpan.FromHours(1),
            TimeUnit.Day => TimeSpan.FromDays(1),
            _ => throw new ArgumentOutOfRangeException(nameof(timeUnit), $"Unexpected time unit value: {timeUnit}")
        };

    /// <summary>
    /// Converts a <see cref="TimeUnit"/> into a "time has passed" event.
    /// </summary>
    /// <param name="timeUnit">The time unit.</param>
    /// <param name="now">The current time.</param>
    /// <param name="previous">The previous time reference.</param>
    /// <returns>A derived <see cref="TimeHasPassed"/> event.</returns>
    public static TimeHasPassed ToEvent(this TimeUnit timeUnit, DateTimeOffset now, DateTimeOffset? previous) =>
        timeUnit switch
        {
            TimeUnit.Minute => new MinuteHasPassed(now, previous),
            TimeUnit.Hour => new HourHasPassed(now, previous),
            TimeUnit.Day => new DayHasPassed(now, previous),
            _ => throw new ArgumentOutOfRangeException(nameof(timeUnit), $"Unexpected time unit value: {timeUnit}")
        };
}
