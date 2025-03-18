namespace Core.OptimisticConcurrency;

/// <summary>
/// Provides an expected resource version.
/// </summary>
public interface IExpectedResourceVersionProvider
{
    string? Value { get; }

    /// <summary>
    /// Attempts to set the expected resource version.
    /// </summary>
    bool TrySet(string value);
}

/// <summary>
/// Default implementation for the expected resource version provider.
/// </summary>
public class ExpectedResourceVersionProvider : IExpectedResourceVersionProvider
{
    public string? Value { get; private set; }

    public bool TrySet(string value)
    {
        // Reject if the value is null, empty, or only whitespace.
        if (string.IsNullOrWhiteSpace(value))
            return false;

        Value = value;
        return true;
    }
}

/// <summary>
/// Provides the next resource version.
/// </summary>
public interface INextResourceVersionProvider
{
    string? Value { get; }

    /// <summary>
    /// Attempts to set the next resource version.
    /// </summary>
    bool TrySet(string value);
}

/// <summary>
/// Default implementation for the next resource version provider.
/// </summary>
public class NextResourceVersionProvider : INextResourceVersionProvider
{
    public string? Value { get; private set; }

    public bool TrySet(string newValue)
    {
        // Reject if the new value is null, empty, or only whitespace.
        if (string.IsNullOrWhiteSpace(newValue))
            return false;

        Value = newValue;
        return true;
    }
}
