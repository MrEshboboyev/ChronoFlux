using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.EntityFramework.Queries;

/// <summary>
/// Provides extension methods for registering query handlers with Entity Framework.
/// </summary>
public static class QueryHandler
{
    /// <summary>
    /// Registers a query handler that operates on a single <typeparamref name="TResult"/>.
    /// </summary>
    public static IServiceCollection AddEntityFrameworkQueryHandler<TDbContext, TQuery, TResult>(
        this IServiceCollection services,
        Func<IQueryable<TResult>, TQuery, CancellationToken, Task<TResult>> handler)
        where TDbContext : DbContext
        where TResult : class =>
        services.AddEntityFrameworkQueryHandler<TDbContext, TResult, TQuery, TResult>(handler);

    /// <summary>
    /// Registers a query handler that operates on a projection/view model (<typeparamref name="TView"/>).
    /// </summary>
    public static IServiceCollection AddEntityFrameworkQueryHandler<TDbContext, TView, TQuery, TResult>(
        this IServiceCollection services,
        Func<IQueryable<TView>, TQuery, CancellationToken, Task<TResult>> handler)
        where TDbContext : DbContext
        where TView : class =>
        services.AddQueryHandler<TQuery, TResult>(sp =>
        {
            // Obtain a queryable set of the specified entity type from the DbContext.
            var queryable = sp.GetRequiredService<TDbContext>()
                .Set<TView>()
                .AsNoTracking() // Disable tracking to optimize read operations.
                .AsQueryable();

            // Return a function that processes the query.
            return (query, ct) => handler(queryable, query, ct);
        });

    /// <summary>
    /// Registers a query handler that returns a list of results.
    /// </summary>
    public static IServiceCollection AddEntityFrameworkQueryHandler<TDbContext, TQuery, TResult>(
        this IServiceCollection services,
        Func<IQueryable<TResult>, TQuery, CancellationToken, Task<IReadOnlyList<TResult>>> handler)
        where TDbContext : DbContext
        where TResult : class =>
        services.AddQueryHandler<TQuery, IReadOnlyList<TResult>>(sp =>
        {
            // Create an IQueryable from the specified result type.
            var queryable = sp.GetRequiredService<TDbContext>()
                .Set<TResult>()
                .AsNoTracking()
                .AsQueryable();

            return (query, ct) => handler(queryable, query, ct);
        });

    /// <summary>
    /// Adds a generic query handler to the service collection.
    /// </summary>
    public static IServiceCollection AddQueryHandler<TQuery, TResult>(
        this IServiceCollection services,
        Func<IServiceProvider, Func<TQuery, CancellationToken, Task<TResult>>> setup) =>
        services.AddTransient(setup);
}
