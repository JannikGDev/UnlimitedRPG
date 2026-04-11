using UnlimitedRPG.Core.Interfaces;

namespace UnlimitedRPG.Stubs;

/// <summary>Echoes the input prompt back. No LLM required.</summary>
public class StubTextGenerator : ITextGenerator
{
    public Task<string> GenerateAsync(string systemPrompt, string inputPrompt, CancellationToken ct = default)
        => Task.FromResult(inputPrompt);
}
