using Ardalis.Specification;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Customers.Specifications;

public sealed class CompanyCustomersSpec: Specification<User>
{
    public CompanyCustomersSpec(Guid tenantId)
    {
        Query
            .Where(u =>
                u.TenantId == tenantId &&
                u.UserRoles.Any(ur =>
                    ur.Role.Name == Role.CompanyCustomer));
    }
}