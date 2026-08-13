using Ardalis.Specification;
using FeedInsight.Domain.Users;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Customers.Specifications
{
    public sealed class CompanyCustomerByIdSpec : SingleResultSpecification<User>
    {
        public CompanyCustomerByIdSpec(Guid customerId, Guid tenantId)
        {
            Query
                .Where(u =>
                    u.Id == customerId &&
                    u.TenantId == tenantId &&
                    u.UserRoles.Any(ur =>
                        ur.Role.Name == Role.CompanyCustomer))
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role);
        }
    }
}
