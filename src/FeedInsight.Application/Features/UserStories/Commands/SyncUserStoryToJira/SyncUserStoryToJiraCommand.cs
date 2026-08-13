using ErrorOr;
using FeedInsight.Application.Messaging;
using System;

namespace FeedInsight.Application.Features.UserStories.Commands.SyncUserStoryToJira;

public record SyncUserStoryToJiraCommand(
    Guid UserStoryId) : IRequest<ErrorOr<Success>>;
