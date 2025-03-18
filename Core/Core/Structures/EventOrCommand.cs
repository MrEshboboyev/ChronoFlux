namespace Core.Structures;

/// <summary>
/// Specialization of Either to distinguish between events and commands.
/// </summary>
public class EventOrCommand : Either<object, object>
{
    // Factory method for creating an event.
    public static EventOrCommand Event(object @event) =>
        new(Maybe<object>.Of(@event), Maybe<object>.Empty);

    // Factory method for creating multiple events.
    public static IEnumerable<EventOrCommand> Events(params object[] events) =>
        events.Select(Event);

    // Factory method for creating multiple events from an enumerable.
    public static IEnumerable<EventOrCommand> Events(IEnumerable<object> events) =>
        events.Select(Event);

    // Factory method for creating a command.
    public static EventOrCommand Command(object @event) =>
        new(Maybe<object>.Empty, Maybe<object>.Of(@event));

    // Private constructor ensuring appropriate use of Maybe values.
    private EventOrCommand(Maybe<object> left, Maybe<object> right) : base(left, right)
    {
    }
}
