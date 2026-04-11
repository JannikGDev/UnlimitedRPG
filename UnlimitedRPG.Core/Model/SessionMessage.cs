namespace UnlimitedRPG.Core.Model;

public class SessionMessage
{
    public Guid     Id        { get; set; } = Guid.NewGuid();
    public Guid     SessionId { get; init; }
    public string   Role      { get; init; } = string.Empty; // "player" | "game"
    public string   Mode      { get; init; } = string.Empty; // "say" | "do" — only set for player messages
    public string   Text      { get; init; } = string.Empty;
    public DateTime SentAt    { get; init; } = DateTime.UtcNow;
}
