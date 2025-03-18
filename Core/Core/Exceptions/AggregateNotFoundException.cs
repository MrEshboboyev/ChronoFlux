namespace Core.Exceptions;

/// <summary>
/// Exception thrown when a requested aggregate is not found.
/// </summary>
public class AggregateNotFoundException : Exception
{
    // Private constructor ensures exceptions are created via the helper methods.
    private AggregateNotFoundException(string typeName, string id)
        : base($"{typeName} with id '{id}' was not found")
    {
    }

    /// <summary>
    /// Creates an AggregateNotFoundException for the given generic aggregate type and Guid id.
    /// </summary>
    public static AggregateNotFoundException For<T>(Guid id) =>
        For<T>(id.ToString());

    /// <summary>
    /// Creates an AggregateNotFoundException for the given generic aggregate type and string identifier.
    /// </summary>
    public static AggregateNotFoundException For<T>(string id) =>
        new(typeof(T).Name, id);
}
