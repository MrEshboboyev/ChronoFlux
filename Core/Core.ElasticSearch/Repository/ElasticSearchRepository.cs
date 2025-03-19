using Core.Aggregates;
using Core.ElasticSearch.Indices;
using Elastic.Clients.Elasticsearch;

namespace Core.ElasticSearch.Repository;

/// <summary>
/// Defines a contract for an Elasticsearch repository that manages aggregates.
/// </summary>
public interface IElasticSearchRepository<T>
    where T : class, IAggregate
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);
    Task Add(Guid id, T aggregate, CancellationToken cancellationToken);
    Task Update(Guid id, T aggregate, CancellationToken cancellationToken);
    Task Delete(Guid id, T aggregate, CancellationToken cancellationToken);
}

/// <summary>
/// Implements an Elasticsearch repository for managing aggregates.
/// </summary>
public class ElasticSearchRepository<T>(ElasticsearchClient elasticClient) : IElasticSearchRepository<T>
    where T : class, IAggregate
{
    private readonly ElasticsearchClient elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));

    public async Task<T?> Find(Guid id, CancellationToken cancellationToken)
    {
        var response = await elasticClient.GetAsync<T>(id, cancellationToken).ConfigureAwait(false);
        return response?.Source;
    }

    public Task Add(Guid id, T aggregate, CancellationToken cancellationToken) =>
        elasticClient.IndexAsync(aggregate, i => i.Id(id).Index(IndexNameMapper.ToIndexName<T>()), cancellationToken);

    public Task Update(Guid id, T aggregate, CancellationToken cancellationToken) =>
        elasticClient.UpdateAsync<T, object>(IndexNameMapper.ToIndexName<T>(), id, i => i.Doc(aggregate), cancellationToken);

    public Task Delete(Guid id, T aggregate, CancellationToken cancellationToken) =>
        elasticClient.DeleteAsync<T>(id, cancellationToken);
}
