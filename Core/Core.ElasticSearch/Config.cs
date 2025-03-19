using Core.Configuration;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.ElasticSearch;

/// <summary>
/// Represents the configuration for connecting to Elasticsearch.
/// </summary>
public class ElasticSearchConfig
{
    public string Url { get; set; } = default!;
    public string DefaultIndex { get; set; } = default!;
}

/// <summary>
/// Provides extension methods to configure and register Elasticsearch in the dependency injection container.
/// </summary>
public static class ElasticSearchConfigExtensions
{
    private const string DefaultConfigKey = "ElasticSearch";

    /// <summary>
    /// Adds and configures Elasticsearch in the DI container, using settings from the application's configuration.
    /// </summary>
    /// <param name="services">The service collection to register Elasticsearch in.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="config">Optional additional configuration for the Elasticsearch client.</param>
    /// <returns>The updated service collection with Elasticsearch configured.</returns>
    public static IServiceCollection AddElasticsearch(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ElasticsearchClientSettings>? config = null)
    {
        // Retrieve the Elasticsearch configuration from the app configuration.
        var elasticSearchConfig = configuration.GetRequiredConfig<ElasticSearchConfig>(DefaultConfigKey);

        // Build the Elasticsearch client settings with the provided URL and default index.
        var settings = new ElasticsearchClientSettings(new Uri(elasticSearchConfig.Url))
            .DefaultIndex(elasticSearchConfig.DefaultIndex);

        // Apply optional custom configurations.
        config?.Invoke(settings);

        // Create the Elasticsearch client and register it as a singleton in the DI container.
        var client = new ElasticsearchClient(settings);
        return services.AddSingleton(client);
    }
}
