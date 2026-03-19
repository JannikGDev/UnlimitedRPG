namespace UnlimitedRPG.Core.Model;

public class User
{
    public Guid   Id       { get; init; } = Guid.NewGuid();
    public string Username { get; init; } = string.Empty;
    public string Email    { get; init; } = string.Empty;

    public ICollection<PlayerCharacter> Characters { get; init; } = [];
}