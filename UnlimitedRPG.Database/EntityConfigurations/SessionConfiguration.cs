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

        builder.HasOne(s => s.PlayerCharacter)
            .WithMany()
            .HasForeignKey(s => s.PlayerCharacterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
