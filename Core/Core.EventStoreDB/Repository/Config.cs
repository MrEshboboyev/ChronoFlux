using Core.Aggregates;
using Core.OpenTelemetry;
using Core.OptimisticConcurrency;
using Microsoft.Extensions.DependencyInjection;

namespace Core.EventStoreDB.Repository;

/// <summary>
/// Provides extension methods to register the EventStoreDB repository and optionally decorate it
/// with ETag (optimistic concurrency) and telemetry support.
/// </summary>
public static class Config
{
    /// <summary>
    /// Registers an EventStoreDB repository for aggregate type <typeparamref name="T"/> and optionally
    /// decorates it with ETag handling and telemetry.
    /// </summary>
    /// <typeparam name="T">
    /// The aggregate type that implements <see cref="IAggregate"/>.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the repository is added.</param>
    /// <param name="withAppendScope">
    /// If <c>true</c>, decorates the repository with optimistic concurrency (ETag) support.
    /// </param>
    /// <param name="withTelemetry">
    /// If <c>true</c>, decorates the repository with telemetry support by wrapping calls in an activity.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddEventStoreDBRepository<T>(
        this IServiceCollection services,
        bool withAppendScope = true,
        bool withTelemetry = true)
        where T : class, IAggregate
    {
        // Register the core repository implementation.
        services.AddScoped<IEventStoreDBRepository<T>, EventStoreDBRepository<T>>();

        if (withAppendScope)
        {
            // Decorate the repository with an ETag (optimistic concurrency) decorator.
            services.Decorate<IEventStoreDBRepository<T>>(
                (inner, sp) => new EventStoreDBRepositoryWithETagDecorator<T>(
                    inner,
                    sp.GetRequiredService<IExpectedResourceVersionProvider>(),
                    sp.GetRequiredService<INextResourceVersionProvider>())
            );
        }

        if (withTelemetry)
        {
            // Decorate the repository with telemetry support.
            services.Decorate<IEventStoreDBRepository<T>>(
                (inner, sp) => new EventStoreDBRepositoryWithTelemetryDecorator<T>(
                    inner,
                    sp.GetRequiredService<IActivityScope>())
            );
        }

        return services;
    }
}
