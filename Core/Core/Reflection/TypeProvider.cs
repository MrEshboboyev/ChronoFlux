namespace Core.Reflection;

/// <summary>
/// Provides utilities for resolving types dynamically from assemblies.
/// </summary>
public static class TypeProvider
{
    /// <summary>
    /// Determines if a type is likely a record by checking for compiler-generated markers.
    /// </summary>
    /// <param name="objectType">The type to inspect.</param>
    /// <returns>True if the type is likely a record; otherwise, false.</returns>
    private static bool IsRecord(this Type objectType) =>
        // Check for a special clone method that records typically have.
        objectType.GetMethod("<Clone>$") != null ||
        // Alternatively, search for an EqualityContract property with a compiler-generated getter.
        ((TypeInfo)objectType)
            .DeclaredProperties.FirstOrDefault(x => x.Name == "EqualityContract")?
            .GetMethod?.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null;

    /// <summary>
    /// Searches for a type with the given name from any assembly referenced by the entry assembly.
    /// This restricts the search to a known subset of assemblies.
    /// </summary>
    /// <param name="typeName">The full or simple name of the type to find.</param>
    /// <returns>
    /// The first matching type found in any referenced assembly, or null if no match is found.
    /// </returns>
    public static Type? GetTypeFromAnyReferencingAssembly(string typeName)
    {
        // Retrieve the names of assemblies referenced by the entry assembly.
        var referencedAssemblies = Assembly.GetEntryAssembly()?
            .GetReferencedAssemblies()
            .Select(a => a.FullName);

        if (referencedAssemblies == null)
            return null;

        // Search for the first type within the referenced assemblies matching the type name.
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => referencedAssemblies.Contains(a.FullName))
            .SelectMany(a => a.GetTypes().Where(x => x.FullName == typeName || x.Name == typeName))
            .FirstOrDefault();
    }

    /// <summary>
    /// Searches for the first matching type with the given name across all assemblies
    /// loaded in the current AppDomain.
    /// </summary>
    /// <param name="typeName">The full or simple name of the type to locate.</param>
    /// <returns>The matching type, or null if none is found.</returns>
    public static Type? GetFirstMatchingTypeFromCurrentDomainAssembly(string typeName) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes().Where(x => x.FullName == typeName || x.Name == typeName))
            .FirstOrDefault();
}
