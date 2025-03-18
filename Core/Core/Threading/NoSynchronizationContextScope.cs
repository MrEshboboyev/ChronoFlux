namespace Core.Threading;

/// <summary>
/// Provides a mechanism to temporarily remove the current SynchronizationContext.
/// Useful when you want to avoid context capturing in asynchronous code.
/// </summary>
public static class NoSynchronizationContextScope
{
    /// <summary>
    /// Enters a scope where the current synchronization context is removed.
    /// </summary>
    /// <returns>A disposable that restores the original context upon disposal.</returns>
    public static Disposable Enter()
    {
        // Capture the current synchronization context.
        var context = SynchronizationContext.Current;
        // Set the context to null.
        SynchronizationContext.SetSynchronizationContext(null);
        return new Disposable(context);
    }

    /// <summary>
    /// A disposable struct that restores the synchronization context when disposed.
    /// </summary>
    public struct Disposable : IDisposable
    {
        private readonly SynchronizationContext? _originalContext;

        public Disposable(SynchronizationContext? originalContext)
        {
            _originalContext = originalContext;
        }

        /// <summary>
        /// Restores the original synchronization context.
        /// </summary>
        public void Dispose() =>
            SynchronizationContext.SetSynchronizationContext(_originalContext);
    }
}
