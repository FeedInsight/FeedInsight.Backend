using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Infrastructure.Messaging;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());

        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler == null) continue;

            var method = handlerType.GetMethod("HandleAsync");
            
            if (method != null)
            {
                await (Task)method.Invoke(handler, new object[] { domainEvent, cancellationToken });
            }
        }
    }
}
