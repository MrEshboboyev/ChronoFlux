using System.Text;
using System.Text.Json;

namespace Core.Requests;

/// <summary>
/// An abstraction for posting commands to an external service.
/// </summary>
public interface IExternalCommandBus
{
    Task PostAsync<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T : notnull;
    Task PutAsync<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T : notnull;
    Task DeleteAsync<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T : notnull;
}

/// <summary>
/// Implementation of IExternalCommandBus that dispatches commands via HTTP.
/// </summary>
public class ExternalCommandBus : IExternalCommandBus
{
    public Task PostAsync<T>(string url, string path, T command, CancellationToken cancellationToken = default)
        where T : notnull
    {
        // Create a new HTTP client with the specified base address.
        var client = new HttpClient { BaseAddress = new Uri(url) };
        // Serialize the command as JSON and wrap it in a StringContent.
        var json = JsonSerializer.Serialize(command);
        return client.PostAsync(path, new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
    }

    public Task PutAsync<T>(string url, string path, T command, CancellationToken cancellationToken = default)
        where T : notnull
    {
        var client = new HttpClient { BaseAddress = new Uri(url) };
        var json = JsonSerializer.Serialize(command);
        return client.PutAsync(path, new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
    }

    public Task DeleteAsync<T>(string url, string path, T command, CancellationToken cancellationToken = default)
        where T : notnull
    {
        // Note: The command parameter is not serialized here since DELETE requests usually do not include a body.
        var client = new HttpClient { BaseAddress = new Uri(url) };
        return client.DeleteAsync(path, cancellationToken);
    }
}
