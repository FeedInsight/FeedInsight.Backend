using ErrorOr;
using FeedInsight.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Customers.Commands.LockCompanyCustomer
{
    public record LockCompanyCustomerCommand(
     Guid CustomerId,
     string? Reason
 ) : IRequest<ErrorOr<Success>>;
}
