using Ardalis.Specification;
using FeedInsight.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Users.Specifications;

public sealed class ProductOwnersFilterSpec : Specification<User>
{
    public ProductOwnersFilterSpec(string? searchTerm, Guid? tenantId)
    {
        Query.AsNoTracking()
             .Where(u => u.UserRoles.Any(ur => ur.Role!.Name == Role.ProductOwner));

        if (tenantId.HasValue)
        {
            Query.Where(u => u.TenantId == tenantId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            Query.Where(u => u.FirstName.Contains(searchTerm) ||
                             u.LastName.Contains(searchTerm) ||
                             u.Email.Contains(searchTerm) ||
                             (u.Tenant != null && u.Tenant.CompanyName.Contains(searchTerm)));
        }
    }
}