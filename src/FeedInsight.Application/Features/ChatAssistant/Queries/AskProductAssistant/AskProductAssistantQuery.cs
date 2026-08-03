using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.ChatAssistant.Queries.AskProductAssistant;

public record AskProductAssistantQuery(
    Guid SessionId,
    string UserQuestion
) : IRequest<ErrorOr<string>>;
