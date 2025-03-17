namespace Core.Ids;

/// <summary>
/// Interface defining a generator for unique identifiers.
/// </summary>
public interface IIdGenerator
{
    /// <summary>
    /// Generates and returns a new unique identifier.
    /// </summary>
    /// <returns>A new <see cref="Guid"/> representing a unique identifier.</returns>
    Guid New();
}