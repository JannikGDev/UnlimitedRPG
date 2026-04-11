namespace UnlimitedRPG.Core.Model;

public class PlayerCharacter
{
    public Guid   Id          { get; init; } = Guid.NewGuid();
    public string Name        { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int    Hp          { get; init; }
    public int    AttackBonus { get; init; }
    public int    DamageBonus { get; init; }
    public int    ArmorClass  { get; init; }
}
