using ErrorOr;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.CustomerFeedbacks.Development.Dtos;
using FeedInsight.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Development.Queries.GetProductOwnerComments
{
    public record GetProductOwnerCommentsQuery(
    int Page,
    int PageSize
) : IRequest<ErrorOr<PaginatedResult<CustomerFeedbackWithProductOwnerCommentsDto>>>;
}
