using Newtonsoft.Json.Serialization;

namespace Core.Serialization.Newtonsoft;

/// <summary>
/// A custom contract resolver that overrides the default object contract creation to use
/// non-default constructors when available.
/// </summary>
public class NonDefaultConstructorContractResolver : DefaultContractResolver
{
    /// <summary>
    /// Overrides the creation of the JSON object contract to configure it to use a non-default constructor.
    /// </summary>
    /// <param name="objectType">The type for which to create the contract.</param>
    /// <returns>The modified <see cref="JsonObjectContract"/>.</returns>
    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        // Use the base contract creation logic and then override it to use non-default constructors.
        var contract = base.CreateObjectContract(objectType);
        return JsonObjectContractProvider.UsingNonDefaultConstructor(
            contract,
            objectType,
            base.CreateConstructorParameters);
    }
}
