using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitedRPG.Core.Model;

namespace UnlimitedRPG.Database.EntityConfigurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.StartedAt).IsRequired();
        builder.Property(s => s.Status).IsRequired();
        builder.Property(s => s.Round).IsRequired();

        builder.HasOne(s => s.World)
            .WithMany()
            .HasForeignKey(s => s.WorldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PlayerCharacter)
            .WithMany()
            .HasForeignKey(s => s.PlayerCharacterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Enemy)
            .WithOne()
            .HasForeignKey<Enemy>("SessionId")  // shadow FK on dependent side
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.CombatLog)
            .WithOne(c => c.Session)
            .HasForeignKey(c => c.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}