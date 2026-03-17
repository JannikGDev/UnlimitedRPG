namespace RpgFramework.Core.Model;

public class Session
{
    public Guid          Id                  { get; init; } = Guid.NewGuid();
    public DateTime      StartedAt           { get; init; } = DateTime.UtcNow;
    public SessionStatus Status              { get; private set; } = SessionStatus.Active;

    // References — independent lifecycles
    public Guid            WorldId           { get; init; }
    public World           World             { get; init; } = null!;
    public Guid            UserId            { get; init; }
    public User            User              { get; init; } = null!;
    public Guid            PlayerCharacterId { get; init; }
    public PlayerCharacter PlayerCharacter   { get; init; } = null!;

    // Owned — dies with the session
    public Enemy           Enemy             { get; init; } = null!;
    public ICollection<CombatLog> CombatLog  { get; init; } = [];

    public void Complete()  => Status = SessionStatus.Completed;
    public void Abandon()   => Status = SessionStatus.Abandoned;

    public bool IsOver => Status != SessionStatus.Active;
}

public enum SessionStatus { Active, Completed, Abandoned }