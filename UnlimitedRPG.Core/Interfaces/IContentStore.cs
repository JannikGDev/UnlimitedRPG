namespace UnlimitedRPG.Core.Interfaces;

public interface IContentStore
{
    Task SaveAsync<T>(string id, T entity, CancellationToken ct = default);
    Task<T?> GetAsync<T>(string id, CancellationToken ct = default);
}
