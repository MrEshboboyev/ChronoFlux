using Marten;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public abstract class MartenTest: IDisposable
{
    private readonly DocumentStore documentStore;
    protected readonly IDocumentSession DocumentSession;

    protected MartenTest()
    {
        var options = new StoreOptions();
        options.Connection(
            "PORT = 5432; HOST = localhost; TIMEOUT = 15; POOLING = True; DATABASE = 'postgres'; PASSWORD = 'postgres'; USER ID = 'postgres'");
        options.UseNewtonsoftForSerialization(nonPublicMembersStorage: NonPublicMembersStorage.All);
        options.DatabaseSchemaName = options.Events.DatabaseSchemaName = "IntroductionToEventSourcing";

        documentStore = new DocumentStore(options);
        DocumentSession = documentStore.LightweightSession();
    }

    protected Task AppendEvents(Guid streamId, object[] events, CancellationToken ct)
    {
        DocumentSession.Events.Append(
            streamId,
            events
        );
        return DocumentSession.SaveChangesAsync(ct);
    }

    public virtual void Dispose()
    {
        DocumentSession.Dispose();
        documentStore.Dispose();
    }
}
