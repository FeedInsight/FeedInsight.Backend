using Ardalis.Specification;
using FeedInsight.Application.Features.Users.Queries.GetProductOwners;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Specifications;

public sealed class ProductOwnersPaginatedSpec : Specification<User, ProductOwnerDto>
{
    public ProductOwnersPaginatedSpec(string? searchTerm, int page, int pageSize)
    {
        Query.AsNoTracking()
             .Where(u => u.UserRoles.Any(ur => ur.Role!.Name == Role.ProductOwner));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            Query.Where(u => u.FirstName.Contains(searchTerm) ||
                             u.LastName.Contains(searchTerm) ||
                             u.Email.Contains(searchTerm) ||
                             (u.Tenant != null && u.Tenant.CompanyName.Contains(searchTerm)));
        }

        Query.Skip((page - 1) * pageSize)
             .Take(pageSize)
             .OrderByDescending(u => u.CreatedAt);

        Query.Select(u => new ProductOwnerDto(
            u.Id,
            u.FirstName,
            u.LastName,
            u.Email,
            u.Tenant != null ? u.Tenant.CompanyName : "Unknown",
            u.IsLocked,
            u.CreatedAt
        ));
    }
}