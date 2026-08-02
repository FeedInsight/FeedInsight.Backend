using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Jira.Commands.ProcessJiraWebhook;

public record ProcessJiraWebhookCommand(
    Guid TenantId,
    string SignatureHeader,
    string RawPayload
) : IRequest<ErrorOr<Success>>;