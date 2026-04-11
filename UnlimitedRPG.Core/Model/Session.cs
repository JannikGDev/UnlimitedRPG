namespace UnlimitedRPG.Core.Model;

public class Session
{
    public Guid            Id                { get; set; }  = Guid.NewGuid();
    public DateTime        StartedAt         { get; init; } = DateTime.UtcNow;
    public SessionStatus   Status            { get; set; }  = SessionStatus.Active;
    public Guid            PlayerCharacterId { get; init; }
    public PlayerCharacter PlayerCharacter   { get; init; } = null!;
}

public enum SessionStatus { Active, Completed, Abandoned }
