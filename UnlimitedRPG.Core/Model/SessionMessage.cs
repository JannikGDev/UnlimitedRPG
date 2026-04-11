namespace UnlimitedRPG.Core.Model;

public class SessionMessage
{
    public Guid      Id        { get; set; } = Guid.NewGuid();
    public Guid      SessionId { get; init; }
    public string    Mode      { get; init; } = string.Empty;
    public string    Text      { get; init; } = string.Empty;
    public DateTime  SentAt    { get; init; } = DateTime.UtcNow;
}
