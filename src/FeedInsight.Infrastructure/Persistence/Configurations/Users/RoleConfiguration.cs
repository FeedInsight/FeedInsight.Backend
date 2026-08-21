using FeedInsight.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedInsight.Infrastructure.Persistence.Configurations.Users;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.Name).IsUnique();

        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Pre-populate the database with our required roles
        builder.HasData(
            new Role(Role.SuperAdmin) 
            { 
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CreatedAt = seedDate
            },
            new Role(Role.ProductOwner) 
            { 
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                CreatedAt = seedDate
            },
             new Role(Role.CompanyCustomer)
             {
                 Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                 CreatedAt = seedDate
             }
        );
    }
}
