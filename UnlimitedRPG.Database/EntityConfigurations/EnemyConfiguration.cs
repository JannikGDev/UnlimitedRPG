using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgFramework.Core.Model;

namespace UnlimitedRPG.Database.EntityConfigurations;

public class EnemyConfiguration : IEntityTypeConfiguration<Enemy>
{
    public void Configure(EntityTypeBuilder<Enemy> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CurrentHp).IsRequired();
        builder.Property(e => e.Status).IsRequired();

        builder.HasOne(e => e.Template)
            .WithMany()
            .HasForeignKey(e => e.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}