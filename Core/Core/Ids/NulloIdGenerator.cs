namespace Core.Ids;


/// <summary>
/// A simple implementation of <see cref="IIdGenerator"/> that returns a new GUID.
/// </summary>
public class NulloIdGenerator : IIdGenerator
{
    /// <summary>
    /// Generates a new unique identifier using <see cref="Guid.NewGuid"/>.
    /// </summary>
    /// <returns>A newly generated <see cref="Guid"/>.</returns>
    public Guid New() => Guid.NewGuid();
}