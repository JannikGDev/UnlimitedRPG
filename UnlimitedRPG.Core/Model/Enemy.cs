namespace UnlimitedRPG.Core.Model;

public class Enemy
{
    public Guid        Id         { get; set; }  = Guid.NewGuid();
    public int         CurrentHp  { get; set; }  // mutable — takes damage
    public EnemyStatus Status     { get; set; }  // mutable — changes each round

    public Guid          TemplateId { get; init; }
    public EnemyTemplate Template   { get; init; } = null!;

    public void ApplyDamage(int damage)
    {
        CurrentHp = Math.Max(0, CurrentHp - damage);
        Status    = CurrentHp <= 0 ? EnemyStatus.Dead
            : CurrentHp <= 3 ? EnemyStatus.Staggered
            : EnemyStatus.Alive;
    }
}

public enum EnemyStatus { Alive, Staggered, Dead }