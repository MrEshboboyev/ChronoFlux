namespace Core.ProcessManagers;

/// <summary>
/// Base process manager class that uses Guid as an identifier.
/// </summary>
public abstract class ProcessManager : ProcessManager<Guid>, IProcessManager
{
}

/// <summary>
/// Base class to implement a process manager.
/// Stores pending messages internally and provides common logic.
/// </summary>
/// <typeparam name="T">The identifier type.</typeparam>
public abstract class ProcessManager<T> : IProcessManager<T> where T : notnull
{
    // Process manager identifier.
    public T Id { get; protected set; } = default!;

    // Current version, typically used for concurrency control.
    public int Version { get; protected set; }

    // Internal queue of pending EventOrCommand objects (e.g. events to be dispatched or commands to schedule).
    [NonSerialized]
    private readonly Queue<EventOrCommand> scheduledCommands = new();

    /// <summary>
    /// Dequeues all pending messages and clears the queue.
    /// </summary>
    public EventOrCommand[] DequeuePendingMessages()
    {
        var dequeuedEvents = scheduledCommands.ToArray();
        scheduledCommands.Clear();
        return dequeuedEvents;
    }

    /// <summary>
    /// Enqueues an event message for later processing.
    /// </summary>
    /// <param name="event">The event to enqueue.</param>
    protected void EnqueueEvent(object @event) =>
        scheduledCommands.Enqueue(EventOrCommand.Event(@event));

    /// <summary>
    /// Enqueues a command message for scheduling.
    /// </summary>
    /// <param name="event">The command to schedule.</param>
    protected void ScheduleCommand(object @event) =>
        scheduledCommands.Enqueue(EventOrCommand.Command(@event));

    /// <summary>
    /// Applies an event to the process manager.
    /// This method can be overridden by implementers to evolve state.
    /// </summary>
    /// <param name="event">The event to apply.</param>
    public virtual void Apply(object @event)
    {
        // Default is to do nothing.
    }
}
