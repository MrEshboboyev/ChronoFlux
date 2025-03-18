namespace Core.Commands;

/// <summary>
/// An in-memory implementation of <see cref="ICommandBus"/> that retrieves
/// command handlers from the DI container and executes them through a retry policy.
/// It integrates with OpenTelemetry via an activity scope.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InMemoryCommandBus"/> class.
/// </remarks>
public class InMemoryCommandBus(
    IServiceProvider serviceProvider,
    CommandHandlerActivity commandHandlerActivity,
    IActivityScope activityScope,
    AsyncPolicy retryPolicy) : ICommandBus
{
    /// <summary>
    /// Sends a command synchronously by attempting to send the command and throwing an exception if not handled.
    /// </summary>
    public async Task SendAsync<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : notnull
    {
        var wasHandled = await TrySendAsync(command, ct).ConfigureAwait(true);
        if (!wasHandled)
            throw new InvalidOperationException($"Unable to find handler for command '{command.GetType().Name}'");
    }

    /// <summary>
    /// Attempts to send a command. Returns false if no handler is registered.
    /// </summary>
    public async Task<bool> TrySendAsync<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : notnull
    {
        var commandHandler = serviceProvider.GetService<ICommandHandler<TCommand>>();
        if (commandHandler == null)
            return false;

        await retryPolicy.ExecuteAsync(token =>
            commandHandlerActivity.TrySendAsync<TCommand>(
                activityScope,
                commandHandler.GetType().Name,
                (_, c) => commandHandler.HandleAsync(command, c),
                token),
            ct).ConfigureAwait(false);

        return true;
    }
}

/// <summary>
/// Provides extension methods to register the in-memory command bus and its supporting components.
/// </summary>
public static class EventBusExtensions
{
    /// <summary>
    /// Registers the <see cref="InMemoryCommandBus"/> together with CommandHandlerMetrics and CommandHandlerActivity in DI.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="asyncPolicy">
    /// An optional Polly async policy. A NoOp policy is used if none is provided.
    /// </param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInMemoryCommandBus(this IServiceCollection services, AsyncPolicy? asyncPolicy = null)
    {
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<CommandHandlerActivity>();
        services.AddScoped(sp =>
            new InMemoryCommandBus(
                sp,
                sp.GetRequiredService<CommandHandlerActivity>(),
                sp.GetRequiredService<IActivityScope>(),
                asyncPolicy ?? Policy.NoOpAsync()
            ))
            .TryAddScoped<ICommandBus>(sp => sp.GetRequiredService<InMemoryCommandBus>());
        return services;
    }
}
