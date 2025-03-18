namespace Core.Events;

/// <summary>
/// Provides mapping utilities for stream names associated with aggregates.
/// Helps generate stream prefixes and canonical stream identifiers.
/// </summary>
public class StreamNameMapper
{
    private static readonly StreamNameMapper Instance = new();
    private readonly ConcurrentDictionary<Type, string> typeNameMap = new();

    /// <summary>
    /// Adds a custom mapping for a stream type using the specified mapped stream name.
    /// </summary>
    /// <typeparam name="TStream">The stream type.</typeparam>
    /// <param name="mappedStreamName">The custom stream name mapping.</param>
    public static void AddCustomMap<TStream>(string mappedStreamName) =>
        AddCustomMap(typeof(TStream), mappedStreamName);

    /// <summary>
    /// Adds a custom mapping for the provided stream type.
    /// </summary>
    public static void AddCustomMap(Type streamType, string mappedStreamName)
    {
        Instance.typeNameMap.AddOrUpdate(streamType, mappedStreamName, (_, _) => mappedStreamName);
    }

    /// <summary>
    /// Returns the stream prefix for the given stream type.
    /// </summary>
    public static string ToStreamPrefix<TStream>() => ToStreamPrefix(typeof(TStream));

    /// <summary>
    /// Generates a stream prefix from the stream type.
    /// By default, the prefix is "{module}_{TypeName}".
    /// </summary>
    public static string ToStreamPrefix(Type streamType) =>
        Instance.typeNameMap.GetOrAdd(streamType, _ =>
        {
            var modulePrefix = streamType.Namespace!.Split('.').First();
            return $"{modulePrefix}_{streamType.Name}";
        });

    /// <summary>
    /// Generates a stream identifier given the aggregate id and an optional tenant id.
    /// Uses the canonical format "{streamPrefix}-{tenantPrefix}{aggregateId}".
    /// </summary>
    /// <typeparam name="TStream">The stream type.</typeparam>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="tenantId">Optional tenant identifier.</param>
    /// <returns>A canonical stream identifier.</returns>
    public static string ToStreamId<TStream>(object aggregateId, object? tenantId = null) =>
        ToStreamId(typeof(TStream), aggregateId, tenantId);

    /// <summary>
    /// Generates a stream id based on the provided stream type, aggregate id, and optional tenant id.
    /// Format: "{streamPrefix}-{tenantPrefix}{aggregateId}".
    /// </summary>
    /// <param name="streamType">The stream type.</param>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="tenantId">Optional tenant identifier.</param>
    /// <returns>A stream identifier string.</returns>
    public static string ToStreamId(Type streamType, object aggregateId, object? tenantId = null)
    {
        var tenantPrefix = tenantId != null ? $"{tenantId}_" : "";
        var streamCategory = ToStreamPrefix(streamType);
        // Note: Placing tenantPrefix on the right-hand side of the '-' allows the category to parse correctly.
        return $"{streamCategory}-{tenantPrefix}{aggregateId}";
    }
}
