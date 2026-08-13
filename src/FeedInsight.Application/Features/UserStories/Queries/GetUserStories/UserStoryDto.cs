using FeedInsight.Domain.UserStories.Enums;
using System;

namespace FeedInsight.Application.Features.UserStories.Queries.GetUserStories;

public record UserStoryDto(
    Guid Id,
    Guid TenantId,
    Guid CategoryId,
    UserStorySource Source,
    string? JiraTicketKey,
    string Title,
    string? AcceptanceCriteria,
    int UrgencyScore,
    UserStoryStatus Status);
