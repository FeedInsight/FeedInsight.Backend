using FeedInsight.Domain.JiraSubtasks;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.UserStories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations.JiraSubtasks;

public class JiraSubtaskConfiguration : IEntityTypeConfiguration<JiraSubtask>
{
    public void Configure(EntityTypeBuilder<JiraSubtask> builder)
    {
        builder.HasKey(js => js.Id);

        builder.Property(js => js.JiraSubtaskKey).HasMaxLength(50).IsRequired();
        builder.Property(js => js.Title).HasMaxLength(255).IsRequired();
        builder.Property(js => js.Status).HasMaxLength(50).IsRequired();

        builder.HasOne<Tenant>().WithMany().HasForeignKey(js => js.TenantId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<UserStory>().WithMany().HasForeignKey(js => js.UserStoryId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(js => js.JiraSubtaskKey).IsUnique();
        builder.HasIndex(js => js.TenantId);
    }
}