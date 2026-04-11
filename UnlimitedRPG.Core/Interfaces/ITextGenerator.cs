namespace UnlimitedRPG.Core.Interfaces;

public interface ITextGenerator
{
    Task<string> GenerateAsync(string systemPrompt, string inputPrompt, CancellationToken ct = default);
}
