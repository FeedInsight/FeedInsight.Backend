using ErrorOr;
using FeedInsight.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Tenants.Queries.GetJiraIntegration
{
    public record GetJiraIntegrationQuery
    : IRequest<ErrorOr<JiraIntegrationDto>>;
}
