namespace Core.Commands;

/// <summary>
/// Provides extension methods for registering command handlers in the DI container.
/// </summary>
public static class Config
{
    /// <summary>
    /// Registers a command handler with a custom factory function.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TCommandHandler">The concrete command handler type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="create">A factory to create an instance of the command handler.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCommandHandler<TCommand, TCommandHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TCommandHandler> create)
        where TCommandHandler : class, ICommandHandler<TCommand>
    {
        return services.AddTransient<TCommandHandler>()
                       .AddTransient<ICommandHandler<TCommand>>(create);
    }

    /// <summary>
    /// Registers a command handler using the default DI resolution.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TCommandHandler">The concrete command handler type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCommandHandler<TCommand, TCommandHandler>(
        this IServiceCollection services)
        where TCommandHandler : class, ICommandHandler<TCommand>
    {
        return services.AddCommandHandler<TCommand, TCommandHandler>(sp =>
            sp.GetRequiredService<TCommandHandler>());
    }
}
