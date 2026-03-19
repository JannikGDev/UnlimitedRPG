namespace UnlimitedRPG.Core.Model;

public class EnemyTemplate
{
    public Guid   Id          { get; init; } = Guid.NewGuid();
    public string Name        { get; init; } = string.Empty;
    public int    BaseHp      { get; init; }
    public int    AttackBonus { get; init; }
    public int    DamageBonus { get; init; }
    public int    ArmorClass  { get; init; }

    // Back-reference for ORM — not used in domain logic
    public Guid   WorldId     { get; init; }
    public World  World       { get; init; } = null!;

    public Enemy Instantiate() => new()
    {
        TemplateId = Id,
        Template   = this,
        CurrentHp  = BaseHp,
        Status     = EnemyStatus.Alive
    };
}