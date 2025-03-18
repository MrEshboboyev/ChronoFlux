namespace Core.Queries;

/// <summary>
/// Describes the contract for a query bus that dispatches queries to registered query handlers
/// and returns the response asynchronously.
/// </summary>
public interface IQueryBus
{
    /// <summary>
    /// Dispatches a query and retrieves its response asynchronously.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResponse">The type of the expected response.</typeparam>
    /// <param name="query">The query to dispatch.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that resolves to the query response.</returns>
    Task<TResponse> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken ct = default)
        where TQuery : notnull;
}
