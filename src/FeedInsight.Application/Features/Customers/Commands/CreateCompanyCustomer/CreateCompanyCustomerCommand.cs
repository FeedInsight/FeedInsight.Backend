using ErrorOr;
using FeedInsight.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Customers.Commands.CreateCompanyCustomer
{
    public record CreateCompanyCustomerCommand(
     string FirstName,
     string LastName,
     string Email,
     string Password
 ) : IRequest<ErrorOr<Guid>>;
}
