using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitedRPG.Core.Model;

namespace UnlimitedRPG.Database.EntityConfigurations;

public class EnemyTemplateConfiguration : IEntityTypeConfiguration<EnemyTemplate>
{
    public void Configure(EntityTypeBuilder<EnemyTemplate> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.BaseHp).IsRequired();
        builder.Property(t => t.AttackBonus).IsRequired();
        builder.Property(t => t.DamageBonus).IsRequired();
        builder.Property(t => t.ArmorClass).IsRequired();

        builder.HasOne(t => t.World)
            .WithMany(w => w.EnemyTemplates)
            .HasForeignKey(t => t.WorldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
