using Core.ElasticSearch.Indices;
using Core.Events;
using Core.Projections;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;

namespace Core.ElasticSearch.Projections;

/// <summary>
/// Projects events to Elasticsearch indices, mapping events to view entities.
/// </summary>
/// <typeparam name="TEvent">The type of the event.</typeparam>
/// <typeparam name="TView">The type of the view entity.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ElasticSearchProjection{TEvent, TView}"/> class.
/// </remarks>
/// <param name="elasticClient">The Elasticsearch client instance.</param>
/// <param name="getId">Function to retrieve the unique ID for the view entity from the event.</param>
public class ElasticSearchProjection<TEvent, TView>(ElasticsearchClient elasticClient, Func<TEvent, string> getId) : IEventHandler<EventEnvelope<TEvent>>
    where TView : class, IProjection
    where TEvent : notnull
{
    private readonly ElasticsearchClient elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    private readonly Func<TEvent, string> getId = getId ?? throw new ArgumentNullException(nameof(getId));

    /// <summary>
    /// Handles an event by retrieving or creating a view entity, applying the event to it,
    /// and indexing it in Elasticsearch.
    /// </summary>
    public async Task HandleAsync(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct)
    {
        var id = getId(eventEnvelope.Data);
        var indexName = IndexNameMapper.ToIndexName<TView>();

        // Retrieve or create the entity.
        var entity = (await elasticClient.GetAsync<TView>(id, i => i.Index(indexName), ct).ConfigureAwait(false))?.Source ??
                     (TView)Activator.CreateInstance(typeof(TView), true)!;

        entity.Apply(eventEnvelope);

        // Index the updated entity in Elasticsearch.
        await elasticClient.IndexAsync(
            entity,
            i => i.Index(indexName).Id(id).VersionType(VersionType.External).Version((long)eventEnvelope.Metadata.StreamPosition),
            ct
        ).ConfigureAwait(false);
    }
}

/// <summary>
/// Provides extension methods to register Elasticsearch projections in the DI container.
/// </summary>
public static class ElasticSearchProjectionConfig
{
    /// <summary>
    /// Registers an Elasticsearch projection for a given event and view type.
    /// </summary>
    public static IServiceCollection Project<TEvent, TView>(
        this IServiceCollection services,
        Func<TEvent, string> getId)
        where TView : class, IProjection
        where TEvent : notnull
    {
        services.AddTransient<IEventHandler<EventEnvelope<TEvent>>>(sp =>
        {
            var elasticClient = sp.GetRequiredService<ElasticsearchClient>();
            return new ElasticSearchProjection<TEvent, TView>(elasticClient, getId);
        });

        return services;
    }
}
