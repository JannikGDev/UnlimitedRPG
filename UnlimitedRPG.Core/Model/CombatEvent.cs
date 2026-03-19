namespace UnlimitedRPG.Core.Model;

public record CombatEvent(
    int  Round,
    bool Hit,
    int  Damage
);
