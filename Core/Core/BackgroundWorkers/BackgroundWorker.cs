namespace Core.BackgroundWorkers;

/// <summary>
/// Generic background worker that wraps a service instance.
/// It translates a service-specific asynchronous function into a common signature.
/// </summary>
/// <typeparam name="TService">The type of the service to use in the background task.</typeparam>
public class BackgroundWorker<TService>(
    TService service,
    ILogger<BackgroundWorker> logger,
    Func<TService, CancellationToken, Task> perform)
    : BackgroundWorker(logger, ct => perform(service, ct))
{
    // This constructor simply passes the service-specific function to the non-generic base.
}

/// <summary>
/// Implements a background service by running the provided asynchronous function.
/// It logs when the background worker starts and when it stops.
/// </summary>
public class BackgroundWorker(
    ILogger<BackgroundWorker> logger,
    Func<CancellationToken, Task> perform)
    : BackgroundService
{
    /// <summary>
    /// The main execution point of the background service.
    /// Executes the provided asynchronous function and logs start/stop events.
    /// </summary>
    /// <param name="stoppingToken">Token signifying when to stop execution.</param>
    /// <returns>A task representing the background operation.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(async () =>
        {
            // Yield control to ensure asynchronous execution.
            await Task.Yield();

            // Log that the background worker has started.
            logger.LogInformation("Background worker started");

            // Execute the supplied asynchronous operation.
            await perform(stoppingToken).ConfigureAwait(false);

            // Log that the background worker has completed execution.
            logger.LogInformation("Background worker stopped");
        }, stoppingToken);
}
