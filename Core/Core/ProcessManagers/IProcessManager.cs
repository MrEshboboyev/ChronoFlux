namespace Core.ProcessManagers;

/// <summary>
/// Non-generic process manager interface with a Guid identifier.
/// </summary>
public interface IProcessManager : IProcessManager<Guid>
{
}

/// <summary>
/// Generic process manager interface that extends IProjection.
/// </summary>
/// <typeparam name="T">The type of the process manager identifier.</typeparam>
public interface IProcessManager<out T> : IProjection
{
    /// <summary>
    /// Unique identifier for the process manager.
    /// </summary>
    T Id { get; }

    /// <summary>
    /// Current version for optimistic concurrency purposes.
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Retrieves and clears any pending commands or events.
    /// </summary>
    EventOrCommand[] DequeuePendingMessages();
}
