using FeedInsight.Domain.Common.Interfaces;

namespace FeedInsight.Domain.UserStories.Events;

public sealed record UserStoryCreatedEvent(Guid TenantId, Guid UserStoryId) : IDomainEvent;
