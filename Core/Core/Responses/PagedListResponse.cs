namespace Core.Responses;

/// <summary>
/// Represents a paginated response with a subset of items and pagination metadata.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
/// <remarks>
/// Constructs a new paginated list response.
/// </remarks>
/// <param name="items">The items in the current page.</param>
/// <param name="totalItemCount">The total count of items.</param>
/// <param name="hasNextPage">A flag indicating the presence of a subsequent page.</param>
public class PagedListResponse<T>(IEnumerable<T> items, long totalItemCount, bool hasNextPage)
{
    /// <summary>
    /// Gets a read-only list of items in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; } = items.ToList().AsReadOnly();

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public long TotalItemCount { get; } = totalItemCount;

    /// <summary>
    /// Indicates whether there is a next page available.
    /// </summary>
    public bool HasNextPage { get; } = hasNextPage;
}
