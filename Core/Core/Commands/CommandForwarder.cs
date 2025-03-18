namespace Core.Commands;

/// <summary>
/// A sample command forwarder that implements an outbox pattern for the command bus.
/// Note: This is an example using EventStoreDB.
/// For production use mature tooling like Wolverine, MassTransit, or NServiceBus.
/// </summary>
/// <typeparam name="T">The type of command to forward.</typeparam>
/// <remarks>
/// Creates a new instance of the <see cref="CommandForwarder{T}"/>.
/// </remarks>
/// <param name="commandBus">The command bus to forward commands to.</param>
public class CommandForwarder<T>(ICommandBus commandBus) : IEventHandler<T>
    where T : notnull
{
    /// <summary>
    /// Forwards the command by calling the command bus's TrySendAsync method.
    /// </summary>
    /// <param name="command">The command to forward.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync(T command, CancellationToken ct)
    {
        await commandBus.TrySendAsync(command, ct).ConfigureAwait(false);
    }
}

/// <summary>
/// Provides extension methods to register the CommandForwarder for DI.
/// </summary>
public static class CommandForwarderConfig
{
    /// <summary>
    /// Registers a transient command forwarder that binds IEventHandler&lt;T&gt; to CommandForwarder&lt;T&gt;.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCommandForwarder(this IServiceCollection services) =>
        services.AddTransient(typeof(IEventHandler<>), typeof(CommandForwarder<>));
}
