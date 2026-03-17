using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgFramework.Core.Model;

namespace UnlimitedRPG.Database.EntityConfigurations;

public class WorldConfiguration : IEntityTypeConfiguration<World>
{
    public void Configure(EntityTypeBuilder<World> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name).IsRequired();

        builder.HasMany(w => w.EnemyTemplates)
            .WithOne(t => t.World)
            .HasForeignKey(t => t.WorldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
