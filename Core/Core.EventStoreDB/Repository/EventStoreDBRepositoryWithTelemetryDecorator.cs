using Core.Aggregates;
using Core.OpenTelemetry;

namespace Core.EventStoreDB.Repository;

/// <summary>
/// Decorates an <see cref="IEventStoreDBRepository{T}"/> instance to add telemetry
/// by wrapping repository calls in an OpenTelemetry activity.
/// </summary>
/// <typeparam name="T">The aggregate type that implements <see cref="IAggregate"/>.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="EventStoreDBRepositoryWithTelemetryDecorator{T}"/> class.
/// </remarks>
/// <param name="inner">The inner repository instance.</param>
/// <param name="activityScope">The activity scope used to create telemetry activities.</param>
public class EventStoreDBRepositoryWithTelemetryDecorator<T>(
    IEventStoreDBRepository<T> inner,
    IActivityScope activityScope) : IEventStoreDBRepository<T>
    where T : class, IAggregate
{

    /// <inheritdoc />
    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        inner.Find(id, cancellationToken);

    /// <inheritdoc />
    public Task<ulong> Add(Guid id, T aggregate, CancellationToken cancellationToken = default) =>
        // Wrap the Add operation in a telemetry activity.
        activityScope.RunAsync($"EventStoreDBRepository/{nameof(Add)}",
        (_, ct) => inner.Add(id, aggregate, ct),
        new StartActivityOptions
        {
            Tags =
            {
                { TelemetryTags.Logic.EntityType, typeof(T).Name },
                { TelemetryTags.Logic.EntityId, id },
                { TelemetryTags.Logic.EntityVersion, aggregate.Version }
            }
        },
        cancellationToken);

    /// <inheritdoc />
    public Task<ulong> Update(Guid id, T aggregate, ulong? expectedVersion = null, CancellationToken token = default) =>
        activityScope.RunAsync($"EventStoreDBRepository/{nameof(Update)}",
        (_, ct) => inner.Update(id, aggregate, expectedVersion, ct),
        new StartActivityOptions
        {
            Tags =
            {
                { TelemetryTags.Logic.EntityType, typeof(T).Name },
                { TelemetryTags.Logic.EntityId, id },
                { TelemetryTags.Logic.EntityVersion, aggregate.Version }
            }
        },
        token);

    /// <inheritdoc />
    public Task<ulong> Delete(Guid id, T aggregate, ulong? expectedVersion = null, CancellationToken token = default) =>
        activityScope.RunAsync($"EventStoreDBRepository/{nameof(Delete)}",
        (_, ct) => inner.Delete(id, aggregate, expectedVersion, ct),
        new StartActivityOptions
        {
            Tags =
            {
                { TelemetryTags.Logic.EntityType, typeof(T).Name },
                { TelemetryTags.Logic.EntityId, id },
                { TelemetryTags.Logic.EntityVersion, aggregate.Version }
            }
        },
        token);
}
