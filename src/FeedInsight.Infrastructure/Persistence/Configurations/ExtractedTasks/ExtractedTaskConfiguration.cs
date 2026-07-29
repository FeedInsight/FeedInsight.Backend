using FeedInsight.Domain.Categories;
using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.ExtractedTasks;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.UserStories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations.ExtractedTasks;

public class ExtractedTaskConfiguration : IEntityTypeConfiguration<ExtractedTask>
{
    public void Configure(EntityTypeBuilder<ExtractedTask> builder)
    {
        builder.HasKey(et => et.Id);

        builder.Property(et => et.ExtractedIntent)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(et => et.TechnicalKeywords)
            .HasMaxLength(500);

        builder.Property(et => et.SyncStatus)
            .HasConversion<string>() // Saves enum as string (e.g., "PendingPOReview")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(et => et.JiraSubtaskKey)
            .HasMaxLength(50);

        // Tenant Relationship
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(et => et.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // Feedback Relationship (No explicit navigation property needed right now)
        builder.HasOne<CustomerFeedback>()
            .WithMany()
            .HasForeignKey(et => et.CustomerFeedbackId)
            .OnDelete(DeleteBehavior.Restrict);

        // Category Relationship
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(et => et.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserStory>()
            .WithMany()
            .HasForeignKey(et => et.UserStoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for fast lookups
        builder.HasIndex(et => et.TenantId);
        builder.HasIndex(et => et.UserStoryId); // Extremely important for the PO Dashboard queries
        builder.HasIndex(et => et.SyncStatus); // Important for polling the pending queue
    }
}