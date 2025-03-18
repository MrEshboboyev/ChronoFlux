namespace Core.Configuration;

/// <summary>
/// Provides extension methods for retrieving configuration values that must exist.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Retrieves a required configuration section and converts it to the specified type.
    /// Throws an exception if the configuration is missing.
    /// </summary>
    /// <typeparam name="T">The expected type of the configuration section.</typeparam>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="configurationKey">The key name for the configuration section.</param>
    /// <returns>An instance of T read from configuration.</returns>
    public static T GetRequiredConfig<T>(this IConfiguration configuration, string configurationKey) =>
        configuration.GetRequiredSection(configurationKey).Get<T>()
               ?? throw new InvalidOperationException(
                   $"{typeof(T).Name} configuration wasn't found for '{configurationKey}' key");

    /// <summary>
    /// Retrieves a required connection string by key.
    /// Throws an exception if the connection string is not found.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="configurationKey">The key for the connection string.</param>
    /// <returns>The connection string.</returns>
    public static string GetRequiredConnectionString(this IConfiguration configuration, string configurationKey) =>
        configuration.GetConnectionString(configurationKey)
        ?? throw new InvalidOperationException(
            $"Configuration string with name '{configurationKey}' was not found");
}
