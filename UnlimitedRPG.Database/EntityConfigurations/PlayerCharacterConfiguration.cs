using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitedRPG.Core.Model;

namespace UnlimitedRPG.Database.EntityConfigurations;

public class PlayerCharacterConfiguration : IEntityTypeConfiguration<PlayerCharacter>
{
    public void Configure(EntityTypeBuilder<PlayerCharacter> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.Hp).IsRequired();
        builder.Property(p => p.AttackBonus).IsRequired();
        builder.Property(p => p.DamageBonus).IsRequired();
        builder.Property(p => p.ArmorClass).IsRequired();
    }
}
