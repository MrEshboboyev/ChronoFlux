namespace Core.Aggregates;


/// <summary>
/// Convenience abstract class for aggregates that use 'object' as the event type
/// and Guid as the identifier.
/// </summary>
public abstract class Aggregate : Aggregate<object, Guid>
{
}

/// <summary>
/// Convenience abstract class for aggregates that process specific event types
/// while using a Guid for the identifier.
/// </summary>
/// <typeparam name="TEvent">The event type processed by the aggregate.</typeparam>
public abstract class Aggregate<TEvent> : Aggregate<TEvent, Guid>
    where TEvent : class
{
}

/// <summary>
/// Base implementation for aggregates that handles:
/// - Storage of unique identifier and version.
/// - Queuing of events that are raised but not yet committed.
/// - Immediate invocation of event application for state updates.
/// </summary>
/// <typeparam name="TEvent">The event type processed by the aggregate.</typeparam>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
public abstract class Aggregate<TEvent, TId> : IAggregate<TEvent>
    where TEvent : class
    where TId : notnull
{
    // The aggregate's unique identifier.
    public TId Id { get; protected set; } = default!;

    // The current version of the aggregate; typically incremented after each event.
    public int Version { get; protected set; }

    // Internal queue to hold events that have been raised but not yet committed.
    [NonSerialized]
    private readonly Queue<TEvent> uncommittedEvents = new();

    /// <summary>
    /// Applies an event to update the aggregate's state.
    /// Override this in derived classes to provide specific state transition logic.
    /// </summary>
    /// <param name="event">The event to be applied.</param>
    public virtual void Apply(TEvent @event)
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Retrieves all uncommitted events, clears the internal event queue,
    /// and returns the events as an array of objects.
    /// </summary>
    /// <returns>An array of uncommitted events.</returns>
    public object[] DequeueUncommittedEvents()
    {
        // Convert the internal queue to an object array.
        var dequeuedEvents = uncommittedEvents.Cast<object>().ToArray();

        // Clear the queue now that the events have been retrieved.
        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    /// <summary>
    /// Enqueues an event as uncommitted and applies it immediately to the aggregate.
    /// </summary>
    /// <param name="event">The event to queue and apply.</param>
    protected void Enqueue(TEvent @event)
    {
        // Add the event to the uncommitted events queue.
        uncommittedEvents.Enqueue(@event);
        // Update the aggregate's state by applying the event.
        Apply(@event);
    }
}
