using FeedInsight.Domain.Common.Interfaces;

namespace FeedInsight.Domain.UserStories.Events;

public sealed record UserStoryUpdatedEvent(Guid TenantId, Guid UserStoryId) : IDomainEvent;
