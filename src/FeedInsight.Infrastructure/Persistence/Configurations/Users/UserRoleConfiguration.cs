using FeedInsight.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations.Users;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        // Composite Primary Key since it doesn't inherit from Entity
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        // Many-to-Many Configuration
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);


        var superAdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var superAdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(new
        {
            UserId = superAdminUserId,
            RoleId = superAdminRoleId,
            AssignedAt = seedDate
        });
    }
}
