namespace Core.Events;

/// <summary>
/// Abstraction for publishing events to internal event handlers.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event envelope asynchronously.
    /// </summary>
    /// <param name="eventEnvelope">The event envelope to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync(IEventEnvelope eventEnvelope, CancellationToken ct);
}

/// <summary>
/// Implementation of an event bus which publishes events using dependency-injected handlers.
/// It supports both envelope-level and raw event-level handling.
/// </summary>
/// <remarks>
/// Creates a new instance of the EventBus.
/// </remarks>
/// <param name="serviceProvider">The root service provider.</param>
/// <param name="activityScope">An activity scope for telemetry.</param>
/// <param name="retryPolicy">A Polly async retry policy to wrap event processing.</param>
public class EventBus(
    IServiceProvider serviceProvider,
    IActivityScope activityScope,
    AsyncPolicy retryPolicy) : IEventBus
{

    // Cache of generic PublishAsync<TEvent> methods keyed by event type.
    private static readonly ConcurrentDictionary<Type, MethodInfo> PublishMethods = new();

    /// <summary>
    /// Generic method to publish an event envelope of type TEvent.
    /// It creates a service scope, starts telemetry activities, and calls all registered handlers.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="eventEnvelope">The event envelope containing the event data and metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task PublishAsync<TEvent>(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct)
        where TEvent : notnull
    {
        using var scope = serviceProvider.CreateScope();
        var eventName = eventEnvelope.Data.GetType().Name;

        // Configure telemetry options with event tag.
        var activityOptions = new StartActivityOptions
        {
            Tags = { { TelemetryTags.EventHandling.Event, eventName } }
        };

        // Get handlers that accept the entire envelope.
        var envelopeHandlers = scope.ServiceProvider.GetServices<IEventHandler<EventEnvelope<TEvent>>>();
        foreach (var handler in envelopeHandlers)
        {
            var activityName = $"{handler.GetType().Name}/{eventName}";
            await activityScope.RunAsync(
                activityName,
                (_, token) => retryPolicy.ExecuteAsync(c => handler.HandleAsync(eventEnvelope, c), token),
                activityOptions,
                ct
            ).ConfigureAwait(false);
        }

        // Get handlers that accept only the event data.
        var eventHandlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();
        foreach (var handler in eventHandlers)
        {
            var activityName = $"{handler.GetType().Name}/{eventName}";
            await activityScope.RunAsync(
                activityName,
                (_, token) => retryPolicy.ExecuteAsync(c => handler.HandleAsync(eventEnvelope.Data, c), token),
                activityOptions,
                ct
            ).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Publishes an event envelope by dynamically finding and invoking the matching generic PublishAsync method.
    /// </summary>
    /// <param name="eventEnvelope">The event envelope to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task PublishAsync(IEventEnvelope eventEnvelope, CancellationToken ct) =>
        // Use reflection to retrieve and invoke the generic method.
        (Task)GetGenericPublishFor(eventEnvelope)
            .Invoke(this, [eventEnvelope, ct])!;

    /// <summary>
    /// Retrieves the cached generic PublishAsync method for the event type, or creates and caches it if not found.
    /// </summary>
    /// <param name="eventEnvelope">The event envelope containing event type information.</param>
    /// <returns>A MethodInfo for PublishAsync&lt;TEvent&gt;.</returns>
    private static MethodInfo GetGenericPublishFor(IEventEnvelope @event) =>
        PublishMethods.GetOrAdd(@event.Data.GetType(), eventType =>
            typeof(EventBus)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == nameof(PublishAsync) && m.GetGenericArguments().Length != 0)
                .MakeGenericMethod(eventType)
        );
}

/// <summary>
/// Extension methods to register the EventBus and supporting batch handler in the DI container.
/// </summary>
public static class EventBusExtensions
{
    /// <summary>
    /// Registers the EventBus with the IServiceCollection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="asyncPolicy">
    /// An optional async Polly policy. If not provided, a NoOp policy is used.
    /// </param>
    /// <returns>The modified IServiceCollection.</returns>
    public static IServiceCollection AddEventBus(this IServiceCollection services, AsyncPolicy? asyncPolicy = null)
    {
        services.AddSingleton(sp => new EventBus(
            sp,
            sp.GetRequiredService<IActivityScope>(),
            asyncPolicy ?? Policy.NoOpAsync()
        ));
        services.AddScoped<EventBusBatchHandler, EventBusBatchHandler>();
        services.AddScoped<IEventBatchHandler>(sp => sp.GetRequiredService<EventBusBatchHandler>());
        services.TryAddSingleton<IEventBus>(sp => sp.GetRequiredService<EventBus>());

        return services;
    }
}
