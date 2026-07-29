using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Tenants.Commands.RevokeApiKey;

public record RevokeApiKeyCommand(
    Guid ApiKeyId
) : IRequest<ErrorOr<Success>>;