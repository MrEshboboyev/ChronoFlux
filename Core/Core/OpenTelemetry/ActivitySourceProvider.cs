namespace Core.OpenTelemetry;

/// <summary>
/// Provides a centralized ActivitySource for creating activities used in tracing.
/// </summary>
public static class ActivitySourceProvider
{
    // The default name for the activity source.
    public const string DefaultSourceName = "chronoflux.net";

    // A globally shared ActivitySource instance.
    public static readonly ActivitySource Instance = new(DefaultSourceName, "v1");

    /// <summary>
    /// Adds a dummy ActivityListener that listens to all activities and returns
    /// the specified sampling result. Useful for verifying that activities are emitted.
    /// </summary>
    /// <param name="samplingResult">
    /// The sampling result to be applied to each activity. Default is to record all data.
    /// </param>
    /// <returns>The configured ActivityListener.</returns>
    public static ActivityListener AddDummyListener(
        ActivitySamplingResult samplingResult = ActivitySamplingResult.AllDataAndRecorded
    )
    {
        // Create an ActivityListener that listens to all sources.
        var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            // Determine the sampling result for each activity.
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => samplingResult
        };

        // Register the listener with the global ActivitySource.
        ActivitySource.AddActivityListener(listener);

        return listener;
    }
}
