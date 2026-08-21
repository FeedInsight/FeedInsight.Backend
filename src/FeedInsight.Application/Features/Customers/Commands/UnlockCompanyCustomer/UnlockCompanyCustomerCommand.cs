using ErrorOr;
using FeedInsight.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Customers.Commands.UnlockCompanyCustomer
{
    public record UnlockCompanyCustomerCommand(
     Guid CustomerId
 ) : IRequest<ErrorOr<Success>>;
}
