using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Tenants.Commands.ConfigureJira;

public class ConfigureJiraCommand : IRequest<ErrorOr<Success>>
{
    public string JiraUrl { get; set; } = string.Empty;
    public string PersonalAccessToken { get; set; } = string.Empty;
    public string WebHookSecret { get; set; } = string.Empty;
}
