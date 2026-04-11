namespace UnlimitedRPG.Core.Model;

public class PlayerCharacter
{
    public Guid   Id          { get; set; } = Guid.NewGuid();
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int    Hp          { get; set; }
    public int    AttackBonus { get; set; }
    public int    DamageBonus { get; set; }
    public int    ArmorClass  { get; set; }
}
