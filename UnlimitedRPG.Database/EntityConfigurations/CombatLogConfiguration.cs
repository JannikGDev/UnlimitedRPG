namespace UnlimitedRPG.Database.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitedRPG.Core.Model;

public class CombatLogConfiguration : IEntityTypeConfiguration<CombatLog>
{
    public void Configure(EntityTypeBuilder<CombatLog> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Round).IsRequired();
        builder.Property(c => c.Hit).IsRequired();
        builder.Property(c => c.Damage).IsRequired();
        builder.Property(c => c.Narration).IsRequired();
        builder.Property(c => c.Provider).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Timestamp).IsRequired();

        builder.HasOne(c => c.Session)
            .WithMany(s => s.CombatLog)
            .HasForeignKey(c => c.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}