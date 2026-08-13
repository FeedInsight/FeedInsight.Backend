using ErrorOr;
using FeedInsight.Application.Messaging;
using System;

namespace FeedInsight.Application.Features.UserStories.Commands.EditFeedInsightUserStory;

public record EditFeedInsightUserStoryCommand(
    Guid UserStoryId,
    string Title,
    string? AcceptanceCriteria) : IRequest<ErrorOr<Success>>;
