using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Tenants.Queries.GetJiraIntegration
{
    public record JiraIntegrationDto(
      string? JiraBaseUrl,
      bool IsPersonalAccessTokenConfigured,
      bool IsWebhookSecretConfigured);
}
