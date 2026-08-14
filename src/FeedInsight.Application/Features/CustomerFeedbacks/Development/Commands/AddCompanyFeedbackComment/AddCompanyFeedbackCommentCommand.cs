using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Commands.Development.AddCompanyFeedbackComment;

public record AddCompanyFeedbackCommentCommand(
    Guid FeedbackId,
    string Content
) : IRequest<ErrorOr<Guid>>;