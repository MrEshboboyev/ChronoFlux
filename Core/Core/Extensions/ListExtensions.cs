namespace Core.Extensions;

/// <summary>
/// Provides extensions methods for working with IList instances.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Replaces an element in the list with a new value.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to modify.</param>
    /// <param name="existingElement">The element to replace.</param>
    /// <param name="replacement">The new element to insert.</param>
    /// <returns>The same list instance with the element replaced.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the element is not found.</exception>
    public static IList<T> Replace<T>(this IList<T> list, T existingElement, T replacement)
    {
        int index = list.IndexOf(existingElement);
        if (index == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(existingElement), "Element was not found");
        }
        list[index] = replacement;
        return list;
    }
}
