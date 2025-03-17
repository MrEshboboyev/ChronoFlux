namespace Core.Decider;

/// <summary>
/// A record that encapsulates logic to decide and evolve state within an event sourcing system.
/// </summary>
/// <typeparam name="TState">The type representing the state of the aggregate.</typeparam>
/// <typeparam name="TCommand">The type representing commands input to the system.</typeparam>
/// <typeparam name="TEvent">The type representing events emitted by the system.</typeparam>
public record Decider<TState, TCommand, TEvent>(
    // Function that produces one or more events from a command and a given state.
    Func<TCommand, TState, TEvent[]> Decide,

    // Function that evolves the state by applying a single event.
    Func<TState, TEvent, TState> Evolve,

    // Function that returns the initial state for the aggregate.
    Func<TState> GetInitialState
);
