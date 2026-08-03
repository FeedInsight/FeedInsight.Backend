using FeedInsight.Domain.Common.Interfaces;

namespace FeedInsight.Application.Messaging;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
