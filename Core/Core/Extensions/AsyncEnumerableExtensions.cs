using Open.ChannelExtensions; // Assumes this extension library is referenced

namespace Core.Extensions;

/// <summary>
/// Provides extension methods for IAsyncEnumerable sequences.
/// </summary>
public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Batches elements from an <see cref="IAsyncEnumerable{T}"/> into batches of a given size,
    /// enforcing a maximum time deadline per batch.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="enumerable">The asynchronous sequence to batch.</param>
    /// <param name="batchSize">The maximum number of elements per batch.</param>
    /// <param name="deadline">The maximum time to wait for a batch to fill.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>An asynchronous sequence of batches (as lists) of elements.</returns>
    public static IAsyncEnumerable<List<T>> BatchAsync<T>(
        this IAsyncEnumerable<T> enumerable,
        int batchSize,
        TimeSpan deadline,
        CancellationToken ct)
    {
        return enumerable
            .ToChannel(cancellationToken: ct)  // Convert async enumerable into a channel
            .Batch(batchSize)                  // Batch items by batch size
            .WithTimeout(deadline)             // Apply a timeout to batch completion
            .AsAsyncEnumerable(cancellationToken: ct); // Convert back to async enumerable
    }
}
