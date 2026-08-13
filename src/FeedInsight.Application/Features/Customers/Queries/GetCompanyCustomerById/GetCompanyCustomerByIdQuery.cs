using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Customers.Queries.GetCompanyCustomerById;

public record GetCompanyCustomerByIdQuery(
    Guid CustomerId
) : IRequest<ErrorOr<CompanyCustomerDto>>;