namespace UnlimitedRPG.Core.Interfaces;

public interface ILlmAdapter
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken ct = default);
}
