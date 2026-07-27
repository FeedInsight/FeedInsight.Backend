using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Tenants.Commands.CreateApiKey;

public class CreateApiKeyCommand : IRequest<ErrorOr<string>>
{
    public string Name { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}
