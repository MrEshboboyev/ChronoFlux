using System.Collections.ObjectModel;

namespace Core.Extensions;

/// <summary>
/// Provides extension methods to allow resolving keyed services as dictionaries.
/// These extensions enable the resolution of dependencies registered with a key.
/// </summary>
public static class DIExtensions
{
    /// <summary>
    /// Configures the <see cref="IServiceCollection"/> to allow keyed services to be resolved as dictionaries.
    /// </summary>
    /// <param name="sc">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AllowResolvingKeyedServicesAsDictionary(this IServiceCollection sc)
    {
        // Register the keyed service cache which collects all keys for a service type.
        sc.AddSingleton(typeof(KeyedServiceCache<,>));

        // Register the service collection itself for keyed service cache dependencies.
        sc.AddSingleton(sc);

        // Register our keyed service dictionary resolver as transient.
        sc.AddTransient(typeof(IDictionary<,>), typeof(KeyedServiceDictionary<,>));
        sc.AddTransient(typeof(IReadOnlyDictionary<,>), typeof(KeyedServiceDictionary<,>));

        return sc;
    }

    /// <summary>
    /// A read-only dictionary implementation backed by keyed services.
    /// </summary>
    /// <typeparam name="TKey">The key type; must be non-nullable.</typeparam>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <remarks>
    /// Initializes a new instance by populating the dictionary using the service keys.
    /// </remarks>
    /// <param name="keys">The keyed service cache.</param>
    /// <param name="provider">The service provider.</param>
    private sealed class KeyedServiceDictionary<TKey, TService>(DIExtensions.KeyedServiceCache<TKey, TService> keys, IServiceProvider provider) : ReadOnlyDictionary<TKey, TService>(CreateDictionary(keys, provider))
        where TKey : notnull
        where TService : notnull
    {
        private static Dictionary<TKey, TService> CreateDictionary(KeyedServiceCache<TKey, TService> keys, IServiceProvider provider)
        {
            var dict = new Dictionary<TKey, TService>(capacity: keys.Keys.Length);
            foreach (var key in keys.Keys)
            {
                dict[key] = provider.GetRequiredKeyedService<TService>(key);
            }
            return dict;
        }
    }

    /// <summary>
    /// Caches keys for keyed service registrations.
    /// </summary>
    /// <typeparam name="TKey">The key type; must be non-nullable.</typeparam>
    /// <typeparam name="TService">The service type.</typeparam>
    private sealed class KeyedServiceCache<TKey, TService>(IServiceCollection sc)
        where TKey : notnull
        where TService : notnull
    {
        /// <summary>
        /// Gets the cached keys for the specified service type.
        /// </summary>
        public TKey[] Keys { get; } = (from service in sc
                                       where service.ServiceKey != null
                                       where service.ServiceKey!.GetType() == typeof(TKey)
                                       where service.ServiceType == typeof(TService)
                                       select (TKey)service.ServiceKey!).ToArray();
    }
}
