using FeedInsight.Domain.Common.Interfaces;

namespace FeedInsight.Domain.ExtractedTasks.Events;

public sealed record ExtractedTaskCreatedEvent(Guid TenantId, Guid ExtractedTaskId) : IDomainEvent;
