using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.CompanyCustomers.Commands.DeleteCompanyCustomer;

public record DeleteCompanyCustomerCommand(Guid CustomerId)
    : IRequest<ErrorOr<Success>>;