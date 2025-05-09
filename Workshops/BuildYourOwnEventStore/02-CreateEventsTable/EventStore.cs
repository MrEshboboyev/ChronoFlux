using Dapper;
using Npgsql;

namespace EventStoreBasics;

public class EventStore(NpgsqlConnection databaseConnection): IDisposable, IEventStore
{
    public void Init()
    {
        // See more in Greg Young's "Building an Event Storage" article https://cqrs.wordpress.com/documents/building-event-storage/
        CreateStreamsTable();
        CreateEventsTable();
    }

    private void CreateStreamsTable()
    {
        const string createStreamsTableSql =
            @"CREATE TABLE IF NOT EXISTS streams(
                      id             UUID                      NOT NULL    PRIMARY KEY,
                      type           TEXT                      NOT NULL,
                      version        BIGINT                    NOT NULL
                  );";
        databaseConnection.Execute(createStreamsTableSql);
    }

    private void CreateEventsTable() =>
        throw new NotImplementedException("Add here create table sql run with Dapper");

    public void Dispose() =>
        databaseConnection.Dispose();
}
