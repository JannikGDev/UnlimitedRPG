namespace UnlimitedRPG.Core.Model;

public class World
{
    public Guid   Id   { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
}
