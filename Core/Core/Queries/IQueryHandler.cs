namespace Core.Queries;

/// <summary>
/// Defines an interface for handling queries of a specific type and producing a response.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The resulting response type.</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : notnull
{
    /// <summary>
    /// Processes the given query asynchronously.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that resolves to the query response.</returns>
    Task<TResponse> HandleAsync(TQuery request, CancellationToken cancellationToken);
}
