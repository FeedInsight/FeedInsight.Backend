using FeedInsight.Domain.Feeds;
using FeedInsight.Domain.Feeds.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations;

public class FeedConfiguration : IEntityTypeConfiguration<Feed>
{
    public void Configure(EntityTypeBuilder<Feed> builder)
    {
        // 1. Table Name & Primary Key (EF Core infers this, but it's good practice to be explicit)
        builder.ToTable("Feeds");
        builder.HasKey(f => f.Id);

        // (The Global Query Filter has been removed from here because 
        // it is now handled dynamically in ApplicationDbContext for ALL entities!)

        // 2. Value Object Conversion
        builder.Property(f => f.Url)
            .HasConversion(
                feedUrl => feedUrl!.Value,           // Write to DB as string
                value => FeedUrl.Create(value).Value // Read from DB as FeedUrl
            )
            .HasColumnName("Url")
            .HasMaxLength(2048) // Standard max length for URLs
            .IsRequired();

        // 4. Standard Properties
        builder.Property(f => f.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(f => f.Status)
            .HasConversion<string>() // Saves the Enum as text ("Active", "Paused") instead of numbers (0, 1) making DB reads easier
            .HasMaxLength(50)
            .IsRequired();
    }
}
