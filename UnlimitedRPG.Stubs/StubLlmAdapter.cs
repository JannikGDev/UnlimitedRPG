using UnlimitedRPG.Core.Interfaces;

namespace UnlimitedRPG.Stubs;

public class StubLlmAdapter : ILlmAdapter
{
    static readonly string[] _responses =
    [
        "You press forward and your blade finds its mark — the enemy recoils.",
        "A feint left, then a decisive strike. The blow lands clean.",
        "The attack connects with a satisfying crack. The enemy staggers.",
        "Your swing goes wide. The enemy sidesteps at the last moment.",
        "You overextend — the attack glances off harmlessly.",
        "The enemy reads the strike and parries with practiced ease.",
    ];

    public async Task<string> GenerateTextAsync(string prompt, CancellationToken ct = default)
    {
        await Task.Delay(200, ct);
        return _responses[Random.Shared.Next(_responses.Length)];
    }
}
