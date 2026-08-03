using FeedInsight.Domain.Chats;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations.Chats;

public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.Title)
            .IsRequired()
            .HasMaxLength(150);

       

        builder.HasOne(cs => cs.Tenant)
            .WithMany()
            .HasForeignKey(cs => cs.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.User)
            .WithMany()
            .HasForeignKey(cs => cs.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(cs => cs.Messages)
            .WithOne(cm => cm.ChatSession)
            .HasForeignKey(cm => cm.ChatSessionId)
            .OnDelete(DeleteBehavior.Cascade);

       

        builder.HasIndex(cs => cs.TenantId);

        builder.HasIndex(cs => cs.UserId);
    }
}