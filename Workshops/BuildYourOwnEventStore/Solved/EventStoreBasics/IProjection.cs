namespace EventStoreBasics;

public interface IProjection
{
    Type[] Handles { get; }
    void Handle(object @event);
}

public abstract class Projection : IProjection
{
    private readonly Dictionary<Type, Action<object>> handlers = [];

    public Type[] Handles => [.. handlers.Keys];

    protected void Projects<TEvent>(Action<TEvent> action) =>
        handlers.Add(typeof(TEvent), @event => action((TEvent) @event));

    public void Handle(object @event) =>
        handlers[@event.GetType()](@event);
}
