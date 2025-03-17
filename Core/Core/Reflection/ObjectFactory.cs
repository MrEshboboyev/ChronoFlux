namespace Core.Reflection;

/// <summary>
/// Provides a compiled delegate to create instances of type T.
/// It decides whether to use the default constructor or, as a fallback,
/// produce an uninitialized instance.
/// </summary>
public static class ObjectFactory<T>
{
    // Compiled function that either calls the default constructor or returns an uninitialized T.
    public static readonly Func<T> GetDefaultOrUninitialized = Creator();

    /// <summary>
    /// Examines type T and returns an appropriate object creation delegate.
    /// Special handling for strings and value types with default constructors.
    /// </summary>
    /// <returns>A Func that creates an instance of T.</returns>
    private static Func<T> Creator()
    {
        var t = typeof(T);

        // Return an empty string for type 'string'.
        if (t == typeof(string))
            return Expression.Lambda<Func<T>>(Expression.Constant(string.Empty)).Compile();

        // If T has a default constructor, compile an expression to create a new instance.
        if (t.HasDefaultConstructor())
            return Expression.Lambda<Func<T>>(Expression.New(t)).Compile();

        // As a fallback, use RuntimeHelpers to create an uninitialized object.
        return () => (T)RuntimeHelpers.GetUninitializedObject(t);
    }
}

/// <summary>
/// Provides an extension method for the Type class to check the existence of a default constructor.
/// </summary>
public static class ObjectFactory
{
    /// <summary>
    /// Determines if a type has a default (parameterless) constructor.
    /// Value types always have a default constructor.
    /// </summary>
    /// <param name="t">The type to inspect.</param>
    /// <returns>True if a default constructor exists, otherwise false.</returns>
    public static bool HasDefaultConstructor(this Type t) =>
        t.IsValueType ||
        t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null, types: Type.EmptyTypes, modifiers: null) != null;
}
