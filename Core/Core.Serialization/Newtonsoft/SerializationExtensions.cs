using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text;

namespace Core.Serialization.Newtonsoft;

/// <summary>
/// Provides extension methods to configure JSON serialization settings using Json.NET.
/// </summary>
public static class SerializationExtensions
{
    /// <summary>
    /// Configures the provided <see cref="JsonSerializerSettings"/> with default options,
    /// including a custom contract resolver and a string enum converter.
    /// </summary>
    /// <param name="settings">The settings to configure.</param>
    /// <returns>The updated settings instance.</returns>
    public static JsonSerializerSettings WithDefaults(this JsonSerializerSettings settings)
    {
        settings.WithNonDefaultConstructorContractResolver()
                .Converters.Add(new StringEnumConverter());
        return settings;
    }

    /// <summary>
    /// Sets the contract resolver to <see cref="NonDefaultConstructorContractResolver"/>.
    /// </summary>
    /// <param name="settings">The settings to update.</param>
    /// <returns>The updated settings instance.</returns>
    public static JsonSerializerSettings WithNonDefaultConstructorContractResolver(this JsonSerializerSettings settings)
    {
        settings.ContractResolver = new NonDefaultConstructorContractResolver();
        return settings;
    }

    /// <summary>
    /// Adds the provided converters to the <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="settings">The settings to update.</param>
    /// <param name="converters">An array of <see cref="JsonConverter"/> instances.</param>
    /// <returns>The updated settings instance.</returns>
    public static JsonSerializerSettings WithConverters(this JsonSerializerSettings settings, params JsonConverter[] converters)
    {
        foreach (var converter in converters)
        {
            settings.Converters.Add(converter);
        }
        return settings;
    }

    /// <summary>
    /// Deserializes a JSON string into an object of type <typeparamref name="T"/> using default settings.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string.</param>
    /// <param name="converters">Optional additional converters.</param>
    /// <returns>An instance of <typeparamref name="T"/>.</returns>
    public static T FromJson<T>(this string json, params JsonConverter[] converters)
    {
        var settings = new JsonSerializerSettings()
            .WithNonDefaultConstructorContractResolver()
            .WithConverters(converters);
        return JsonConvert.DeserializeObject<T>(json, settings)!;
    }

    /// <summary>
    /// Deserializes a JSON string into an object of the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="type">The target type.</param>
    /// <param name="converters">Optional additional converters.</param>
    /// <returns>The deserialized object, or null if deserialization fails.</returns>
    public static object? FromJson(this string json, Type type, params JsonConverter[] converters)
    {
        var settings = new JsonSerializerSettings()
            .WithNonDefaultConstructorContractResolver()
            .WithConverters(converters);
        return JsonConvert.DeserializeObject(json, type, settings);
    }

    /// <summary>
    /// Serializes an object to a JSON string using default settings.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="converters">Optional additional converters.</param>
    /// <returns>The JSON string representation of the object.</returns>
    public static string ToJson(this object obj, params JsonConverter[] converters)
    {
        var settings = new JsonSerializerSettings()
            .WithNonDefaultConstructorContractResolver()
            .WithConverters(converters);
        return JsonConvert.SerializeObject(obj, settings);
    }

    /// <summary>
    /// Serializes an object to a <see cref="StringContent"/> containing JSON,
    /// using UTF8 encoding and the "application/json" media type.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A <see cref="StringContent"/> instance containing the JSON representation.</returns>
    public static StringContent ToJsonStringContent(this object obj)
    {
        return new StringContent(obj.ToJson(), Encoding.UTF8, "application/json");
    }
}
