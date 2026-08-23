using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Customers.Queries.GetCompanyCustomerById
{
    public record CompanyCustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsLocked
);
}
