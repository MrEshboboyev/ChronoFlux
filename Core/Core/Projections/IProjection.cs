namespace Core.Projections;

/// <summary>
/// Non-generic interface that represents an event projection.
/// </summary>
public interface IProjection
{
    /// <summary>
    /// Applies an event (of any type) to update the projection.
    /// </summary>
    /// <param name="event">The event to apply.</param>
    void Apply(object @event);
}

/// <summary>
/// Generic interface for a projection that expects events of a specific type.
/// Uses default interface implementation to bridge with the non-generic <see cref="IProjection"/>
/// </summary>
/// <typeparam name="TEvent">The specific event type this projection can process.</typeparam>
public interface IProjection<in TEvent> : IProjection where TEvent : class
{
    /// <summary>
    /// Applies an event of type <typeparamref name="TEvent"/> to update the projection.
    /// </summary>
    /// <param name="event">The typed event to apply.</param>
    void Apply(TEvent @event);

    /// <summary>
    /// Default implementation of the non-generic Apply. It checks if the supplied event
    /// matches type <typeparamref name="TEvent"/> and forwards it to the generic Apply method.
    /// </summary>
    /// <param name="event">An object representing the event.</param>
    void IProjection.Apply(object @event)
    {
        // Ensure the event is of the expected type before processing.
        if (@event is TEvent typedEvent)
            Apply(typedEvent);
    }
}

/// <summary>
/// Interface for projections that need to track the last processed event position.
/// </summary>
public interface ITrackLastProcessedPosition
{
    /// <summary>
    /// Gets or sets the position of the last event that was processed.
    /// </summary>
    ulong LastProcessedPosition { get; set; }
}

/// <summary>
/// A versioned projection is one that handles events and keeps track of its processing state.
/// </summary>
public interface IVersionedProjection : IProjection, ITrackLastProcessedPosition
{
    // This interface aggregates IProjection and ITrackLastProcessedPosition.
    // Additional version-specific methods or properties can be added if needed.
}
