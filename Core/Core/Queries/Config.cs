namespace Core.Queries;

/// <summary>
/// Provides extension methods for registering query handlers in the dependency injection container.
/// </summary>
public static class Config
{
    /// <summary>
    /// Registers a transient query handler for the specified query and query result types.
    /// This registration binds <see cref="IQueryHandler{TQuery, TQueryResult}"/> to the concrete
    /// implementation <typeparamref name="TQueryHandler"/>.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TQueryResult">The type of the query result.</typeparam>
    /// <typeparam name="TQueryHandler">The concrete query handler type that implements <see cref="IQueryHandler{TQuery,TQueryResult}"/>.</typeparam>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddQueryHandler<TQuery, TQueryResult, TQueryHandler>(
        this IServiceCollection services)
        where TQuery : notnull
        where TQueryHandler : class, IQueryHandler<TQuery, TQueryResult> =>
            services.AddTransient<TQueryHandler>()
                    .AddTransient<IQueryHandler<TQuery, TQueryResult>>(sp => sp.GetRequiredService<TQueryHandler>());
}
