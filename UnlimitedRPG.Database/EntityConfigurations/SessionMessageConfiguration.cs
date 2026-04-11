using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitedRPG.Core.Model;

namespace UnlimitedRPG.Database.EntityConfigurations;

public class SessionMessageConfiguration : IEntityTypeConfiguration<SessionMessage>
{
    public void Configure(EntityTypeBuilder<SessionMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Mode).IsRequired();
        builder.Property(m => m.Text).IsRequired();
        builder.Property(m => m.SentAt).IsRequired();
    }
}
