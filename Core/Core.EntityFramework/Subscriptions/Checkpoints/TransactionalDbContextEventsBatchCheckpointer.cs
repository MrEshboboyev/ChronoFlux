using Core.EventStoreDB.Subscriptions.Batch;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;
using EventStore.Client;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static Core.EventStoreDB.Subscriptions.Checkpoints.ISubscriptionCheckpointRepository;

namespace Core.EntityFramework.Subscriptions.Checkpoints;

/// <summary>
/// Implements an events batch checkpointer that operates within a transactional DbContext scope.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="TransactionalDbContextEventsBatchCheckpointer{TDbContext}"/> class.
/// </remarks>
public class TransactionalDbContextEventsBatchCheckpointer<TDbContext>(
    TDbContext dbContext,
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    EventsBatchProcessor batchProcessor) : IEventsBatchCheckpointer
    where TDbContext : DbContext
{
    /// <summary>
    /// Processes a batch of events with checkpointing, committing the database transaction if successful.
    /// </summary>
    /// <param name="events">The events to process.</param>
    /// <param name="lastCheckpoint">The most recent checkpoint to update.</param>
    /// <param name="options">Batch processing options.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// A <see cref="StoreResult"/> indicating whether the processing succeeded or failed.
    /// </returns>
    public async Task<StoreResult> Process(
        ResolvedEvent[] events,
        Checkpoint lastCheckpoint,
        BatchProcessingOptions options,
        CancellationToken ct)
    {
        // Use the transaction within the DbContext.
        await dbContext.Database.UseTransactionAsync(transaction, ct);

        // Create an inner batch checkpointer with a Postgres-based checkpoint repository.
        var inner = new EventsBatchCheckpointer(
            new PostgresSubscriptionCheckpointRepository(connection, transaction),
            batchProcessor);

        // Process the batch of events.
        var result = await inner.Process(events, lastCheckpoint, options, ct).ConfigureAwait(false);

        if (result is StoreResult.Success)
        {
            // Commit changes in the DbContext and the transaction.
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct).ConfigureAwait(false);
        }
        else
        {
            // Rollback the transaction if processing fails.
            await transaction.RollbackAsync(ct);
        }

        return result;
    }
}
