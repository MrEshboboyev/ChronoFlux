namespace Core;

/// <summary>
/// Provides an extension method for registering core services related to event sourcing, telemetry,
/// command and query handling, and other shared utilities.
/// </summary>
public static class Config
{
    /// <summary>
    /// Registers a collection of core services into the dependency injection container.
    /// This includes setting up keyed services, telemetry, event bus, command bus, query bus,
    /// and registering essential singleton services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services
            .AllowResolvingKeyedServicesAsDictionary()
            .AddSingleton(TimeProvider.System)
            .AddSingleton(ActivityScope.Instance)
            .AddEventBus()
            .AddInMemoryCommandBus()
            .AddQueryBus();

        services.TryAddScoped<IExternalCommandBus, ExternalCommandBus>();

        services.TryAddScoped<IIdGenerator, NulloIdGenerator>();
        services.TryAddSingleton(EventTypeMapper.Instance);

        return services;
    }
}
