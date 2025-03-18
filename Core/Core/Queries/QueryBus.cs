namespace Core.Queries;

/// <summary>
/// An in-memory implementation of <see cref="IQueryBus"/> that resolves query handlers
/// from the DI container and wraps query processing within an OpenTelemetry activity.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="QueryBus"/> class.
/// </remarks>
/// <param name="serviceProvider">The service provider used to resolve query handlers.</param>
/// <param name="activityScope">The activity scope for telemetry.</param>
/// <param name="retryPolicy">The retry policy for executing queries.</param>
public class QueryBus(
    IServiceProvider serviceProvider,
    IActivityScope activityScope,
    AsyncPolicy retryPolicy) : IQueryBus
{

    /// <summary>
    /// Dispatches a query to its corresponding handler and returns the result.
    /// </summary>
    /// <typeparam name="TQuery">The query type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="query">The query to process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that resolves to the query response.</returns>
    public Task<TResponse> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken ct = default)
        where TQuery : notnull
    {
        // Retrieve the query handler from the DI container.
        var queryHandler =
            serviceProvider.GetService<IQueryHandler<TQuery, TResponse>>()
            ?? throw new InvalidOperationException($"Unable to find handler for Query '{query.GetType().Name}'");

        var queryName = typeof(TQuery).Name;
        var activityName = $"{queryHandler.GetType().Name}/{queryName}";

        // Execute the query within an activity and a retry policy.
        return activityScope.RunAsync(
            activityName,
            (_, token) => retryPolicy.ExecuteAsync(c => queryHandler.HandleAsync(query, c), token),
            new StartActivityOptions
            {
                Tags = { { TelemetryTags.QueryHandling.Query, queryName } }
            },
            ct
        );
    }
}

/// <summary>
/// Provides extension methods for registering the in-memory query bus in the DI container.
/// </summary>
public static class QueryBusExtensions
{
    /// <summary>
    /// Adds a scoped <see cref="IQueryBus"/> to the service collection using an in-memory implementation.
    /// </summary>
    /// <param name="services">The current service collection.</param>
    /// <param name="asyncPolicy">An optional async retry policy; a NoOp policy is used by default if null.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddQueryBus(this IServiceCollection services, AsyncPolicy? asyncPolicy = null)
    {
        services.AddScoped(sp =>
            new QueryBus(
                sp,
                sp.GetRequiredService<IActivityScope>(),
                asyncPolicy ?? Policy.NoOpAsync()
            ))
            .TryAddScoped<IQueryBus>(sp => sp.GetRequiredService<QueryBus>());

        return services;
    }
}
