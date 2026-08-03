using FeedInsight.Domain.Common.Interfaces;

namespace FeedInsight.Domain.CustomerFeedbacks.Events;

public sealed record CustomerFeedbackCreatedEvent(Guid TenantId, Guid CustomerFeedbackId) : IDomainEvent;
