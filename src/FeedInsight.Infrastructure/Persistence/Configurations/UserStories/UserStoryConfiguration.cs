using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.UserStories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations.UserStories;

public class UserStoryConfiguration : IEntityTypeConfiguration<UserStory>
{
    public void Configure(EntityTypeBuilder<UserStory> builder)
    {
        builder.HasKey(us => us.Id);

        builder.Property(us => us.Source).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(us => us.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(us => us.JiraTicketKey).HasMaxLength(50);
        builder.Property(us => us.Title).HasMaxLength(255).IsRequired();
        builder.Property(us => us.AcceptanceCriteria).HasColumnType("nvarchar(max)");

        builder.HasOne<Tenant>().WithMany().HasForeignKey(us => us.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(us => us.Category).WithMany().HasForeignKey(us => us.CategoryId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(us => us.TenantId);
        builder.HasIndex(us => us.JiraTicketKey);
    }
}