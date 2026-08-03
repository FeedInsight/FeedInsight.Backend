using FeedInsight.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations.Tenants;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Prefix)
            .IsRequired()
            .HasMaxLength(12);

        builder.Property(a => a.KeyHash)
            .IsRequired()
            .HasMaxLength(256);

        // Ensure we can quickly look up a key by its hash during API calls!
        builder.HasIndex(a => a.KeyHash).IsUnique();
    }
}
