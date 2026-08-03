using ErrorOr;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Queries.GetCustomerFeedbacks;

public record GetCustomerFeedbacksQuery(
    int Page = 1,
    int PageSize = 10
) : IRequest<ErrorOr<PaginatedResult<CustomerFeedbackDto>>>;
