namespace Core.Aggregates;

/// <summary>
/// Base interface for aggregates that can process events (i.e. projections) and
/// track version information for optimistic concurrency.
/// </summary>
public interface IAggregate : IProjection
{
    // The current version of the aggregate; usually incremented with each applied event.
    int Version { get; }

    // Retrieves (and clears) uncommitted events that have not yet been persisted.
    object[] DequeueUncommittedEvents();
}

/// <summary>
/// Generic version of IAggregate that ensures type safety for event application.
/// </summary>
/// <typeparam name="TEvent">The event type the aggregate processes.</typeparam>
public interface IAggregate<in TEvent> : IAggregate, IProjection<TEvent>
    where TEvent : class
{
}