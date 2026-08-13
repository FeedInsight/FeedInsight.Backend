using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations.CustomerFeedbacks;

public class CustomerFeedbackConfiguration : IEntityTypeConfiguration<CustomerFeedback>
{
    public void Configure(EntityTypeBuilder<CustomerFeedback> builder)
    {
        builder.HasKey(cf => cf.Id);

        builder.Property(cf => cf.RawContent)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(cf => cf.MetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(cf => cf.SubmitterEmail)
            .HasMaxLength(255);

        builder.Property(cf => cf.OverallSentiment)
            .HasMaxLength(20);

        builder.Property(cf => cf.IsProcessedByRouter)
            .IsRequired();

        // --- RELATIONSHIPS ---

        // Every feedback MUST belong to a specific company (Tenant)
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(cf => cf.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cf => cf.SubmitterUser)
            .WithMany()
            .HasForeignKey(cf => cf.SubmitterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- INDEXES ---
        builder.HasIndex(cf => cf.TenantId);

        builder.HasIndex(cf => cf.SubmitterUserId);

        // 2. Queue Index: Semantic Kernel Background Service will constantly poll the database 
        // with the query: SELECT * FROM CustomerFeedbacks WHERE IsProcessedByRouter = 0.
        // This index prevents full-table scans, keeping your database CPU usage near zero.
        builder.HasIndex(cf => cf.IsProcessedByRouter);
    }
}