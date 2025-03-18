using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using System.Reflection;

namespace Core.Serialization.Newtonsoft;

/// <summary>
/// Provides helper methods to configure a <see cref="JsonObjectContract"/> to use a
/// non-default (parameterized) constructor for object creation during JSON deserialization.
/// </summary>
public static class JsonObjectContractProvider
{
    // The attribute type to search for custom constructors.
    private static readonly Type ConstructorAttributeType = typeof(JsonConstructorAttribute);

    // Cache that associates an object's AssemblyQualifiedName with its configured JsonObjectContract.
    private static readonly ConcurrentDictionary<string, JsonObjectContract> Constructors = new();

    /// <summary>
    /// Configures the given <see cref="JsonObjectContract"/> to use a non-default constructor if one is available.
    /// This method caches configuration based on the object type.
    /// </summary>
    /// <param name="contract">The original JSON object contract to modify.</param>
    /// <param name="objectType">The type for which the contract is being generated.</param>
    /// <param name="createConstructorParameters">
    /// A delegate that creates constructor parameters using the found constructor and contract properties.
    /// </param>
    /// <returns>The modified JSON object contract with a non-default constructor, if found; otherwise, the original contract.</returns>
    public static JsonObjectContract UsingNonDefaultConstructor(
        JsonObjectContract contract,
        Type objectType,
        Func<ConstructorInfo, JsonPropertyCollection, IList<JsonProperty>> createConstructorParameters)
    {
        return Constructors.GetOrAdd(objectType.AssemblyQualifiedName!, _ =>
        {
            // Locate a non-default (non-parameterless) constructor.
            var nonDefaultConstructor = GetNonDefaultConstructor(objectType);

            // If no such constructor exists, return the contract unmodified.
            if (nonDefaultConstructor == null)
            {
                return contract;
            }

            // Override the default creator with one that uses the non-default constructor.
            contract.OverrideCreator = GetObjectConstructor(nonDefaultConstructor);
            contract.CreatorParameters.Clear();

            // Populate the creator parameters using the supplied delegate.
            foreach (var constructorParameter in createConstructorParameters(nonDefaultConstructor, contract.Properties))
            {
                contract.CreatorParameters.Add(constructorParameter);
            }

            return contract;
        });
    }

    /// <summary>
    /// Converts the provided constructor method into an <see cref="ObjectConstructor{object}"/> delegate.
    /// </summary>
    /// <param name="method">The constructor method to convert.</param>
    /// <returns>A delegate that invokes the constructor with the provided arguments.</returns>
    private static ObjectConstructor<object> GetObjectConstructor(MethodBase method)
    {
        // If the method is not a constructor, invoke it statically.
        if (method is not ConstructorInfo c)
        {
            return args => method.Invoke(null, args)!;
        }

        // If the constructor takes no parameters, ignore any arguments.
        if (c.GetParameters().Length == 0)
        {
            return _ => c.Invoke([]);
        }

        // Return a delegate that invokes the constructor with the specified arguments.
        return args => c.Invoke(args);
    }

    /// <summary>
    /// Determines a suitable non-default constructor for the given type.
    /// For primitive types and enums, null is returned.
    /// </summary>
    /// <param name="objectType">The type to inspect.</param>
    /// <returns>A non-default <see cref="ConstructorInfo"/> if available; otherwise, null.</returns>
    private static ConstructorInfo? GetNonDefaultConstructor(Type objectType)
    {
        if (objectType.IsPrimitive || objectType.IsEnum)
        {
            return null;
        }

        // Prefer a constructor marked with the JsonConstructor attribute.
        return GetAttributeConstructor(objectType) ?? GetTheMostSpecificConstructor(objectType);
    }

    /// <summary>
    /// Searches for a constructor that is decorated with the <see cref="JsonConstructorAttribute"/>.
    /// </summary>
    /// <param name="objectType">The type to search.</param>
    /// <returns>The constructor associated with the attribute if exactly one is found; otherwise, null.</returns>
    private static ConstructorInfo? GetAttributeConstructor(Type objectType)
    {
        if (objectType.IsPrimitive || objectType.IsEnum)
        {
            return null;
        }

        var constructors = objectType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(c => c.GetCustomAttributes().Any(a => a.GetType() == ConstructorAttributeType))
            .ToList();

        return constructors.Count switch
        {
            1 => constructors[0],
            > 1 => throw new JsonException($"Multiple constructors with a {ConstructorAttributeType.Name}."),
            _ => null
        };
    }

    /// <summary>
    /// Selects the constructor with the highest number of parameters from the specified type.
    /// </summary>
    /// <param name="objectType">The type for which to select a constructor.</param>
    /// <returns>The constructor with the most parameters, or null if not found.</returns>
    private static ConstructorInfo? GetTheMostSpecificConstructor(Type objectType) =>
        objectType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                  .MaxBy(c => c.GetParameters().Length);
}
