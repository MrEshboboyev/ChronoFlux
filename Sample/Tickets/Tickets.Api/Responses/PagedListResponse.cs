using Marten.Pagination;
using Newtonsoft.Json;

namespace Tickets.Api.Responses;

public class PagedListResponse<T>
{
    public IReadOnlyList<T> Items { get; }

    public long TotalItemCount { get; }

    public bool HasNextPage { get; }

    [JsonConstructor]
    internal PagedListResponse(IEnumerable<T> items, long totalItemCount, bool hasNextPage)
    {
        Items = [.. items];
        TotalItemCount = totalItemCount;
        HasNextPage = hasNextPage;
    }
}

public static class PagedListResponse
{
    public static PagedListResponse<T> From<T>(IPagedList<T> pagedList) =>
        new(pagedList, pagedList.TotalItemCount, pagedList.HasNextPage);
}
