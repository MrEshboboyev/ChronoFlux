namespace Core.Events;

/// <summary>
/// Provides mapping between CLR event types and their string representation (names).
/// Facilitates custom mapping and reverse lookups for event types.
/// </summary>
public class EventTypeMapper
{
    /// <summary>
    /// Singleton instance of the EventTypeMapper.
    /// </summary>
    public static readonly EventTypeMapper Instance = new();

    // Maps event type names (string) to the actual Type.
    private readonly ConcurrentDictionary<string, Type?> typeMap = new();
    // Maps event Types to their string names.
    private readonly ConcurrentDictionary<Type, string> typeNameMap = new();

    /// <summary>
    /// Adds a custom mapping for the event type T using the specified name.
    /// </summary>
    public void AddCustomMap<T>(string eventTypeName) => AddCustomMap(typeof(T), eventTypeName);

    /// <summary>
    /// Adds a custom mapping for the provided event type.
    /// </summary>
    /// <param name="eventType">The CLR Type representing the event.</param>
    /// <param name="eventTypeName">The desired mapping name.</param>
    public void AddCustomMap(Type eventType, string eventTypeName)
    {
        typeNameMap.AddOrUpdate(eventType, eventTypeName, (_, _) => eventTypeName);
        typeMap.AddOrUpdate(eventTypeName, eventType, (_, _) => eventType);
    }

    /// <summary>
    /// Returns the mapped name for the specified event type.
    /// </summary>
    public string ToName<TEventType>() => ToName(typeof(TEventType));

    /// <summary>
    /// Returns the mapped name for the given event Type. If no mapping exists,
    /// the event's FullName is used.
    /// </summary>
    public string ToName(Type eventType) => typeNameMap.GetOrAdd(eventType, _ =>
    {
        var eventTypeName = eventType.FullName!;
        typeMap.TryAdd(eventTypeName, eventType);
        return eventTypeName;
    });

    /// <summary>
    /// Retrieves the CLR event Type corresponding to the given event type name.
    /// </summary>
    public Type? ToType(string eventTypeName) => typeMap.GetOrAdd(eventTypeName, _ =>
    {
        var type = TypeProvider.GetFirstMatchingTypeFromCurrentDomainAssembly(eventTypeName);
        if (type != null)
        {
            typeNameMap.TryAdd(type, eventTypeName);
        }
        return type;
    });
}
