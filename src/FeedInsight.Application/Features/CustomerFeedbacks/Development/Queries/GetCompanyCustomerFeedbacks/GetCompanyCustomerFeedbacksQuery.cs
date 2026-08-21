using ErrorOr;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;
using static FeedInsight.Application.Features.CustomerFeedbacks.Development.Queries.GetCompanyCustomerFeedbacks.GetCompanyCustomerFeedbacksQueryHandler;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Development.Queries.GetCompanyCustomerFeedbacks
{
    public record GetCompanyCustomerFeedbacksQuery(
    int Page = 1,
    int PageSize = 10):
        IRequest<ErrorOr<PaginatedResult<CompanyCustomerFeedbackDto>>>;

}
