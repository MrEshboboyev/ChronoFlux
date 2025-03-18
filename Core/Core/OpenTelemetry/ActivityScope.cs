namespace Core.OpenTelemetry;

/// <summary>
/// Provides a contract for creating and running activities (i.e. traces) using OpenTelemetry.
/// </summary>
public interface IActivityScope
{
    // Starts an activity with the given name using default options.
    Activity? Start(string name) =>
        Start(name, new StartActivityOptions());

    // Starts an activity with the specified name and options.
    Activity? Start(string name, StartActivityOptions options);

    // Runs an asynchronous operation within an activity scope using default options.
    Task RunAsync(
        string name,
        Func<Activity?, CancellationToken, Task> run,
        CancellationToken ct
    ) => RunAsync(name, run, new StartActivityOptions(), ct);

    // Runs an asynchronous operation within an activity scope with specified options.
    Task RunAsync(
        string name,
        Func<Activity?, CancellationToken, Task> run,
        StartActivityOptions options,
        CancellationToken ct
    );

    // Runs an asynchronous function returning a TResult within an activity scope using default options.
    Task<TResult> RunAsync<TResult>(
        string name,
        Func<Activity?, CancellationToken, Task<TResult>> run,
        CancellationToken ct
    ) => RunAsync(name, run, new StartActivityOptions(), ct);

    // Runs an asynchronous function returning a TResult within an activity scope with specified options.
    Task<TResult> RunAsync<TResult>(
        string name,
        Func<Activity?, CancellationToken, Task<TResult>> run,
        StartActivityOptions options,
        CancellationToken ct
    );
}

/// <summary>
/// Implements IActivityScope to start activities and run asynchronous work within their context.
/// </summary>
public class ActivityScope : IActivityScope
{
    // A globally accessible instance to be used throughout the application.
    public static readonly IActivityScope Instance = new ActivityScope();

    /// <summary>
    /// Starts an activity with the provided name and options.
    /// If a parent ActivityContext is specified, it creates a child activity;
    /// otherwise, it uses ParentId or the current activity context.
    /// </summary>
    public Activity? Start(string name, StartActivityOptions options) =>
        options.Parent.HasValue
            ? ActivitySourceProvider.Instance
                .CreateActivity(
                    $"{ActivitySourceProvider.DefaultSourceName}.{name}",
                    options.Kind,
                    parentContext: options.Parent.Value,
                    idFormat: ActivityIdFormat.W3C,
                    tags: options.Tags
                )?.Start()  // Start and return the created activity.
            : ActivitySourceProvider.Instance
                .CreateActivity(
                    $"{ActivitySourceProvider.DefaultSourceName}.{name}",
                    options.Kind,
                    parentId: options.ParentId ?? Activity.Current?.ParentId,
                    idFormat: ActivityIdFormat.W3C,
                    tags: options.Tags
                )?.Start();

    /// <summary>
    /// Executes an asynchronous action within the scope of an activity.
    /// The activity's status is marked as Ok on success, or Error on exception.
    /// </summary>
    public async Task RunAsync(
        string name,
        Func<Activity?, CancellationToken, Task> run,
        StartActivityOptions options,
        CancellationToken ct
    )
    {
        // Attempt to start a new activity; if not available, fallback to the current activity.
        using var activity = Start(name, options) ?? Activity.Current;

        try
        {
            await run(activity, ct).ConfigureAwait(false);
            // Indicate that the activity completed successfully.
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            // Mark the activity as failed and attach the exception.
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes an asynchronous function that returns a result within an activity scope.
    /// Sets the activity status based on the outcome.
    /// </summary>
    public async Task<TResult> RunAsync<TResult>(
        string name,
        Func<Activity?, CancellationToken, Task<TResult>> run,
        StartActivityOptions options,
        CancellationToken ct
    )
    {
        using var activity = Start(name, options) ?? Activity.Current;

        try
        {
            var result = await run(activity, ct).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error);
            throw;
        }
    }
}

/// <summary>
/// Options for configuring activity startup.
/// </summary>
public record StartActivityOptions
{
    /// <summary>
    /// Custom key/value pairs to be attached to the activity as tags.
    /// </summary>
    public Dictionary<string, object?> Tags { get; set; } = [];

    /// <summary>
    /// Optional parent id to define the parent-child relationship in traces.
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// Optional ActivityContext to explicitly set the parent.
    /// </summary>
    public ActivityContext? Parent { get; set; }

    /// <summary>
    /// The activity kind, defaulting to Internal.
    /// </summary>
    public ActivityKind Kind = ActivityKind.Internal;
}
