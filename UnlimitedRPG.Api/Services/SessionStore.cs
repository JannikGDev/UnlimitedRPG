using System.Collections.Concurrent;
using UnlimitedRPG.Api.Controllers;

namespace UnlimitedRPG.Api.Services;

/// <summary>
/// In-memory store for active session state.
/// Singleton — shared between the controller and the notification service
/// so narration can be written back after the async LLM call completes.
/// </summary>
public class SessionStore
{
    private readonly ConcurrentDictionary<Guid, SessionStateDto> _sessions = new();

    public bool TryGet(Guid id, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out SessionStateDto? session) =>
        _sessions.TryGetValue(id, out session);

    public void Set(Guid id, SessionStateDto session) =>
        _sessions[id] = session;

    /// <summary>
    /// Finds the combat log entry for <paramref name="round"/> and fills in the narration.
    /// No-op if the session or round is not found.
    /// </summary>
    public void UpdateNarration(Guid id, int round, string narration)
    {
        if (!_sessions.TryGetValue(id, out var session)) return;

        var updatedLog = session.CombatLog
            .Select(e => e.Round == round ? e with { Narration = narration, Provider = "stub" } : e)
            .ToArray();

        _sessions[id] = session with { CombatLog = updatedLog };
    }
}
