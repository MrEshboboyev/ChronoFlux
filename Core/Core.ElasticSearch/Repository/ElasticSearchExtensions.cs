using Elastic.Clients.Elasticsearch;
using System.Collections.Concurrent;

namespace Core.ElasticSearch.Repository;

/// <summary>
/// Provides extension methods for interacting with Elasticsearch repositories, such as finding and upserting entities.
/// </summary>
public static class ElasticSearchRepository
{
    private static readonly ConcurrentDictionary<Type, string> TypeNameMap = new();

    /// <summary>
    /// Finds an entity in Elasticsearch by its identifier.
    /// </summary>
    public static async Task<T?> Find<T>(this ElasticsearchClient elasticClient, string id, CancellationToken ct)
        where T : class =>
        (await elasticClient.GetAsync<T>(id, cancellationToken: ct).ConfigureAwait(false))?.Source;

    /// <summary>
    /// Upserts (updates or inserts) an entity in Elasticsearch.
    /// </summary>
    public static async Task Upsert<T>(this ElasticsearchClient elasticClient, string id, T entity, CancellationToken ct)
        where T : class =>
        await elasticClient.UpdateAsync<T, object>(
            ToIndexName<T>(), id,
            u => u.Doc(entity).Upsert(entity),
            ct
        ).ConfigureAwait(false);

    private static string ToIndexName<TIndex>()
    {
        var indexType = typeof(TIndex);
        return TypeNameMap.GetOrAdd(indexType, _ =>
        {
            var modulePrefix = indexType.Namespace!.Split('.').First();
            return $"{modulePrefix}-{indexType.Name}".ToLower();
        });
    }
}
