using FeedInsight.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations.Tenants;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id); 

        builder.Property(t => t.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.StatusReason)
            .HasMaxLength(500);

        builder.Property(t => t.JiraBaseUrl)
            .HasMaxLength(500);

        builder.Property(t => t.JiraEncryptedToken)
            .HasMaxLength(1000);

        builder.Property(t => t.JiraWebhookSecret)
            .HasMaxLength(1000);

        builder.HasMany(t => t.ApiKeys)
            .WithOne()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Cascade); // if tenant is deleted, delete keys
    }
}
