namespace Core.Extensions;

/// <summary>
/// Provides extension methods for merging and modifying dictionaries.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Merges two dictionaries into a new dictionary.
    /// </summary>
    /// <typeparam name="TKey">The key type; must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="first">The first dictionary.</param>
    /// <param name="second">The second dictionary.</param>
    /// <returns>A new dictionary containing entries from both dictionaries.</returns>
    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        Dictionary<TKey, TValue> first,
        Dictionary<TKey, TValue> second)
        where TKey : notnull =>
            new(first.Union(second));

    /// <summary>
    /// Merges two sequences of key-value pairs into a new dictionary.
    /// </summary>
    /// <typeparam name="TKey">The key type; must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="first">The first sequence of key-value pairs.</param>
    /// <param name="second">The second sequence of key-value pairs.</param>
    /// <returns>A new dictionary containing entries from both sequences.</returns>
    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        IEnumerable<KeyValuePair<TKey, TValue>> first,
        IEnumerable<KeyValuePair<TKey, TValue>> second)
        where TKey : notnull =>
            new(first.Union(second));

    /// <summary>
    /// Creates a new dictionary based on the first dictionary, setting the specified key to the given value.
    /// </summary>
    /// <typeparam name="TKey">The key type; must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="first">The original dictionary.</param>
    /// <param name="key">The key to add or update.</param>
    /// <param name="value">The value to associate with the key.</param>
    /// <returns>A new dictionary with the key-value pair merged in.</returns>
    public static Dictionary<TKey, TValue> With<TKey, TValue>(
        this Dictionary<TKey, TValue> first,
        TKey key,
        TValue value)
        where TKey : notnull
    {
        // Create a copy of the original dictionary.
        var newDictionary = first.ToDictionary(kv => kv.Key, kv => kv.Value);
        // Set or update the key with the new value.
        newDictionary[key] = value;
        return newDictionary;
    }
}
