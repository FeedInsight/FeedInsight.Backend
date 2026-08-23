using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Development.Dtos
{
    public record FeedbackCommentDto(
    Guid Id,
    Guid UserId,
    string Content,
    DateTime CreatedAt
);
}
