using Core.Commands;
using Core.EventStoreDB.Events;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Core.EventStoreDB.Commands;

/// <summary>
/// An example implementation of an asynchronous command bus using EventStoreDB.
/// This shows an outbox pattern for the command bus. For production workloads, consider
/// using mature tooling such as Wolverine, MassTransit, or NServiceBus.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EventStoreDBAsyncCommandBus"/> class.
/// </remarks>
/// <param name="eventStoreClient">The EventStoreDB client instance used to append events.</param>
public class EventStoreDBAsyncCommandBus(EventStoreClient eventStoreClient) : IAsyncCommandBus
{
    // The stream identifier used to store commands externally.
    public static readonly string CommandsStreamId = "commands-external";

    /// <summary>
    /// Schedules a command asynchronously by appending it to the external commands stream.
    /// </summary>
    /// <typeparam name="TCommand">The type of command. Must be not null.</typeparam>
    /// <param name="command">The command to schedule.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous command scheduling operation.</returns>
    public Task ScheduleAsync<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : notnull =>
        eventStoreClient.Append(CommandsStreamId, command, ct);
}

/// <summary>
/// Provides extensions to register the EventStoreDB based asynchronous command bus.
/// </summary>
public static class Config
{
    /// <summary>
    /// Registers <see cref="IAsyncCommandBus"/> as <see cref="EventStoreDBAsyncCommandBus"/>
    /// and also adds the command forwarder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEventStoreDBAsyncCommandBus(this IServiceCollection services) =>
        services.AddScoped<IAsyncCommandBus, EventStoreDBAsyncCommandBus>()
                .AddCommandForwarder();
}
