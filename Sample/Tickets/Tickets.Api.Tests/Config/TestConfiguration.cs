namespace Tickets.Api.Tests.Config;

public static class TestConfiguration
{
    public static Dictionary<string, string> Get(string fixtureName) =>
        new()
        {
            {
                "EventStore:ConnectionString",
                "PORT = 5432; HOST = localhost; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'postgres'; PASSWORD = 'postgres'; USER ID = 'postgres'"
            },
            {"EventStore:WriteModelSchema", $"{fixtureName}Write"},
            {"EventStore:ReadModelSchema", $"{fixtureName}Read"},
            {"EventStore:ShouldRecreateDatabase", "true"}
        };
}
