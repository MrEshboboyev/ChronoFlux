using System.Collections.Concurrent;

namespace Core.ElasticSearch.Indices;

/// <summary>
/// Maps .NET types to Elasticsearch index names and prefixes, supporting custom mappings.
/// </summary>
public class IndexNameMapper
{
    private static readonly IndexNameMapper Instance = new();
    private readonly ConcurrentDictionary<Type, string> typeNameMap = new();

    /// <summary>
    /// Adds a custom mapping for a specific type to a designated Elasticsearch index name.
    /// </summary>
    public static void AddCustomMap<TStream>(string mappedStreamName) =>
        AddCustomMap(typeof(TStream), mappedStreamName);

    /// <summary>
    /// Adds a custom mapping for a type to a custom Elasticsearch index name.
    /// </summary>
    public static void AddCustomMap(Type streamType, string mappedStreamName)
    {
        Instance.typeNameMap.AddOrUpdate(streamType, mappedStreamName, (_, _) => mappedStreamName);
    }

    /// <summary>
    /// Generates the Elasticsearch index prefix for the specified type.
    /// </summary>
    public static string ToIndexPrefix<TStream>() => ToIndexPrefix(typeof(TStream));

    public static string ToIndexPrefix(Type streamType) => Instance.typeNameMap.GetOrAdd(streamType, _ =>
    {
        var modulePrefix = streamType.Namespace!.Split('.').First();
        return $"{modulePrefix}-{streamType.Name}".ToLower();
    });

    /// <summary>
    /// Generates the full index name for the specified type, optionally including a tenant prefix.
    /// </summary>
    public static string ToIndexName<TStream>(object? tenantId = null) =>
        ToIndexName(typeof(TStream), tenantId);

    public static string ToIndexName(Type streamType, object? tenantId = null)
    {
        var tenantPrefix = tenantId != null ? $"{tenantId}-" : "";
        return $"{tenantPrefix}{ToIndexPrefix(streamType)}".ToLower();
    }
}
