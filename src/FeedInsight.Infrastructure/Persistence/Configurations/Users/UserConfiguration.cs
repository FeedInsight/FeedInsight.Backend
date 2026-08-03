using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        // Fast lookup and uniqueness for login
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.IsLocked)
            .IsRequired();

        builder.Property(u => u.LockReason)
            .HasMaxLength(500);

        // One-to-Many: User -> RefreshTokens
        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optional relationship to Tenant (Null for SuperAdmins)
        builder.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        var superAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(new
        {
            Id = superAdminId,
            TenantId = (Guid?)null,
            FirstName = "Super",
            LastName = "Admin",
            Email = "super@admin.com",
            PasswordHash = "$2a$11$HS4tYBhHEWaYg7VhXbIluOdpZ/Dmg4f/LBTrYzj5OeMj9Wi3PubZe",
            IsLocked = false,
            CreatedAt = seedDate,
            IsDeleted = false
        });
    }
}
