using FeedInsight.Domain.CustomerFeedbacks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations;

public class FeedbackCommentConfiguration : IEntityTypeConfiguration<FeedbackComment>
{
    public void Configure(EntityTypeBuilder<FeedbackComment> builder)
    {
        builder.ToTable("FeedbackComments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasOne(x => x.CustomerFeedback)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.CustomerFeedbackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}