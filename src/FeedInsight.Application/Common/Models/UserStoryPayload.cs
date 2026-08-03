using System;

namespace FeedInsight.Application.Common.Models;

public record UserStoryPayload(
    Guid TenantId,
    Guid CategoryId,
    string Source,
    string? JiraTicketKey,
    string Title,
    string? AcceptanceCriteria,
    int UrgencyScore,
    string Status,
    string Text
);
