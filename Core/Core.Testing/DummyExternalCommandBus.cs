using Core.Requests;

namespace Core.Testing;

public class DummyExternalCommandBus : IExternalCommandBus
{
    public IList<object> SentCommands { get; } = new List<object>();

    public Task PostAsync<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: notnull
    {
        SentCommands.Add(command);
        return Task.CompletedTask;
    }

    public Task PutAsync<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: notnull
    {
        SentCommands.Add(command);
        return Task.CompletedTask;
    }

    public Task DeleteAsync<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: notnull
    {
        SentCommands.Add(command);
        return Task.CompletedTask;
    }
}
