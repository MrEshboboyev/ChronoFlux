namespace Core.Reflection;

/// <summary>
/// Provides methods for converting values between types using type converters.
/// </summary>
public static class ValueTypeConverter
{
    /// <summary>
    /// Converts the given object to the specified target type T using the appropriate type converter.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value of type T.</returns>
    public static T ChangeType<T>(object value) =>
        (T)ChangeType(typeof(T), value)!;

    /// <summary>
    /// Converts the provided object to the target type using the type's default converter.
    /// </summary>
    /// <param name="t">The target type.</param>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted object, or null if conversion is not possible.</returns>
    public static object? ChangeType(Type t, object value)
    {
        // Obtain the type converter for the target type.
        var tc = TypeDescriptor.GetConverter(t);
        // Convert the object using the type converter.
        return tc.ConvertFrom(value);
    }

    /// <summary>
    /// Registers a custom type converter for type T by adding a TypeConverterAttribute via TypeDescriptor.
    /// </summary>
    /// <typeparam name="T">The type to add the converter to.</typeparam>
    /// <typeparam name="TC">The custom type converter (must derive from TypeConverter).</typeparam>
    public static void RegisterTypeConverter<T, TC>() where TC : TypeConverter =>
        TypeDescriptor.AddAttributes(typeof(T), new TypeConverterAttribute(typeof(TC)));
}
