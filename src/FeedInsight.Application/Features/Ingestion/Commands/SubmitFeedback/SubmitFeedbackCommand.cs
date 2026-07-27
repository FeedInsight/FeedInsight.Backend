using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Ingestion.Commands.SubmitFeedback;

public class SubmitFeedbackCommand : IRequest<ErrorOr<Guid>>
{
    public string RawContent { get; set; } = string.Empty;
    public string? SubmitterEmail { get; set; }
    public string? MetadataJson { get; set; }
}
