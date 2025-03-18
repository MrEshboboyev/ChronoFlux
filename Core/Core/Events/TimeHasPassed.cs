namespace Core.Events;

/// <summary>
/// Base record representing a passage of time.
/// </summary>
/// <param name="Now">The current point in time.</param>
/// <param name="PreviousTime">Optional previous time for comparison.</param>
public abstract record TimeHasPassed(DateTimeOffset Now, DateTimeOffset? PreviousTime)
{
    /// <summary>
    /// Represents that a minute has passed.
    /// </summary>
    public record MinuteHasPassed(DateTimeOffset Now, DateTimeOffset? PreviousTime)
        : TimeHasPassed(Now, PreviousTime);

    /// <summary>
    /// Represents that an hour has passed.
    /// </summary>
    public record HourHasPassed(DateTimeOffset Now, DateTimeOffset? PreviousTime)
        : TimeHasPassed(Now, PreviousTime);

    /// <summary>
    /// Represents that a day has passed.
    /// </summary>
    public record DayHasPassed(DateTimeOffset Now, DateTimeOffset? PreviousTime)
        : TimeHasPassed(Now, PreviousTime);
}
