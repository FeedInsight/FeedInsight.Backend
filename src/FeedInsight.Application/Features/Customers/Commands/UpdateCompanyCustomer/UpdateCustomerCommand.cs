using ErrorOr;
using FeedInsight.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Customers.Commands.UpdateCompanyCustomer
{
    public record UpdateCustomerCommand(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string Email
) : IRequest<ErrorOr<Success>>;
}
