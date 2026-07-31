using Ardalis.Specification;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Specifications;

public sealed class ProductOwnersSpec : Specification<User>
{
    public ProductOwnersSpec(
        string? searchTerm,
        Guid? tenantId,
        bool? isActive,
        int? page = null,
        int? pageSize = null)
    {
        Query.AsNoTracking()
             .Include(u => u.Tenant)
             .Where(u => u.UserRoles.Any(ur => ur.Role!.Name == Role.ProductOwner));

        if (tenantId.HasValue)
        {
            Query.Where(u => u.TenantId == tenantId.Value);
        }

        if (isActive.HasValue)
        {
            Query.Where(u => u.IsLocked == !isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            Query.Where(u => u.FirstName.Contains(searchTerm) ||
                             u.LastName.Contains(searchTerm) ||
                             u.Email.Contains(searchTerm) ||
                             (u.Tenant != null && u.Tenant.CompanyName.Contains(searchTerm)));
        }

        Query.OrderByDescending(u => u.CreatedAt);

        if (page.HasValue && pageSize.HasValue)
        {
            Query.Skip((page.Value - 1) * pageSize.Value)
                 .Take(pageSize.Value);
        }
    }
}
