using System;
using System.Collections.Generic;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Queries.GetCustomerFeedbacks;

public record CustomerFeedbackDto(
    Guid Id,
    string RawContent,
    string? SubmitterEmail,
    string? MetadataJson,
    string? OverallSentiment,
    bool IsProcessedByRouter,
    DateTime CreatedAt,
    List<ExtractedTaskDto> ExtractedTasks
);

public record ExtractedTaskDto(
    Guid Id,
    Guid CategoryId,
    string? CategoryName,
    Guid? UserStoryId,
    string ExtractedIntent,
    string? TechnicalKeywords,
    string SyncStatus,
    string? JiraSubtaskKey,
    DateTime CreatedAt
);
