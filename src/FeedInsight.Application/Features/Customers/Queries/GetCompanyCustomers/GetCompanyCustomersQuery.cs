using ErrorOr;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Customers.Queries.GetCompanyCustomers;

public record GetCompanyCustomersQuery(
    int Page = 1,
    int PageSize = 10
) : IRequest<ErrorOr<PaginatedResult<CompanyCustomerDto>>>;

public record CompanyCustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsLocked,
    DateTime CreatedAt
);