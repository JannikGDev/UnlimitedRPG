namespace RpgFramework.Core.Model;

public class CombatLog
{
    public Guid     Id        { get; init; } = Guid.NewGuid();
    public int      Round     { get; init; }
    public bool     Hit       { get; init; }
    public int      Damage    { get; init; }
    public string   Narration { get; set; } = string.Empty;
    public string   Provider  { get; set; } = string.Empty; // "stub" | "claude"
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    // Back-reference
    public Guid    SessionId  { get; init; }
    public Session Session    { get; init; } = null!;
}