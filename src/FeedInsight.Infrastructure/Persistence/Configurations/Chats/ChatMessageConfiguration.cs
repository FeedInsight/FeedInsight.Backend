using FeedInsight.Domain.Chats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations.Chats;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(cm => cm.SenderRole)
            .IsRequired();

        builder.HasOne(cm => cm.Tenant)
            .WithMany()
            .HasForeignKey(cm => cm.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.ChatSession)
            .WithMany(cs => cs.Messages)
            .HasForeignKey(cm => cm.ChatSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cm => cm.TenantId);

        builder.HasIndex(cm => cm.ChatSessionId);
    }
}