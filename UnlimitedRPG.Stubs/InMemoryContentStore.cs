using System.Collections.Concurrent;
using System.Text.Json;
using UnlimitedRPG.Core.Interfaces;

namespace UnlimitedRPG.Stubs;

public class InMemoryContentStore : IContentStore
{
    readonly ConcurrentDictionary<string, string> _store = new();

    public Task SaveAsync<T>(string id, T entity, CancellationToken ct = default)
    {
        _store[id] = JsonSerializer.Serialize(entity);
        return Task.CompletedTask;
    }

    public Task<T?> GetAsync<T>(string id, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var json))
            return Task.FromResult(JsonSerializer.Deserialize<T>(json));
        return Task.FromResult<T?>(default);
    }
}
