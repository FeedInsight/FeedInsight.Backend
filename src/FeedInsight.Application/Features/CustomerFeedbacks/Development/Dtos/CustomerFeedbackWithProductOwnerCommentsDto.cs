using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Development.Dtos
{
    public record CustomerFeedbackWithProductOwnerCommentsDto(
    Guid Id,
    string RawContent,
    string? SubmitterEmail,
    string? MetadataJson,
    string? OverallSentiment,
    bool IsProcessedByRouter,
    DateTime CreatedAt,
    List<ProductOwnerCommentDto> ProductOwnerComments
);

    public record ProductOwnerCommentDto(
        Guid Id,
        Guid UserId,
        string Content,
        DateTime CreatedAt
    );
}
