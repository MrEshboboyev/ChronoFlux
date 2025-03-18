namespace Core.Validation;

/// <summary>
/// Provides fluent validation extension methods for method parameters.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Ensures that a value type is not null; returns the value or throws an exception.
    /// </summary>
    public static T NotNull<T>(this T? value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : struct
    {
        if (value == null)
            throw new ArgumentNullException(paramName);
        return value.Value;
    }

    /// <summary>
    /// Ensures that a reference type is not null; returns the value or throws an exception.
    /// </summary>
    public static T NotNull<T>(this T? value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : class
    {
        if (value == null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    /// <summary>
    /// Verifies that a string is not null, empty, or whitespace.
    /// </summary>
    public static string NotEmpty(this string? value, [CallerArgumentExpression("value")] string? paramName = null) =>
        !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentOutOfRangeException(paramName);

    /// <summary>
    /// Ensures that a Guid is not empty.
    /// </summary>
    public static Guid NotEmpty(this Guid? value, [CallerArgumentExpression("value")] string? paramName = null) =>
        value != null && value != Guid.Empty ? value.Value : throw new ArgumentOutOfRangeException(paramName);

    /// <summary>
    /// For value types, ensures that the value is not the default value.
    /// </summary>
    public static T NotEmpty<T>(this T value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : struct
        => NotEmpty((T?)value, paramName);

    /// <summary>
    /// Runs an assertion action on the value and returns the value.
    /// </summary>
    public static T Has<T>(this T value, Action<T> assert)
    {
        assert(value);
        return value;
    }

    /// <summary>
    /// Ensures that the nullable value type is not null and not the default.
    /// </summary>
    public static T NotEmpty<T>(this T? value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : struct
    {
        var notNullValue = value.NotNull(paramName);
        if (Equals(notNullValue, default(T)))
            throw new ArgumentOutOfRangeException(paramName);
        return notNullValue;
    }

    /// <summary>
    /// Asserts that a comparable value is greater than or equal to the specified threshold.
    /// </summary>
    public static T GreaterOrEqualThan<T>(this T value, T valueToCompare, [CallerArgumentExpression("value")] string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(valueToCompare) < 0)
            throw new ArgumentOutOfRangeException(paramName);
        return value;
    }

    /// <summary>
    /// Validates that a numeric value is positive.
    /// </summary>
    public static T Positive<T>(this T value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : INumber<T>
    {
        // Although value is a non-nullable struct, we compare against zero.
        if (value.CompareTo(T.Zero) <= 0)
            throw new ArgumentOutOfRangeException(paramName);
        return value;
    }
}
