using FeedInsight.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Infrastructure.Messaging;

public class Mediator : IMediator
{
    private readonly IServiceScopeFactory _scopeFactory;

    public Mediator(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        // Create a scope to resolve scoped services (like Validators and Behaviors)
        using (var scope = _scopeFactory.CreateScope())
        {
            var provider = scope.ServiceProvider;
            var requestType = request.GetType();

            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

            // Resolve from the scoped provider
            var handler = provider.GetService(handlerType);

            if (handler == null)
            {
                throw new InvalidOperationException($"No handler registered for type {requestType.Name}");
            }

            var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));

            // Resolve behaviors from the scoped provider
            var behaviors = provider.GetServices(behaviorType)
                                    .ToList();

            RequestHandlerDelegate<TResponse> next = () =>
            {
                var method = handlerType.GetMethod("HandleAsync");
                return (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken });
            };

            for (int i = behaviors.Count - 1; i >= 0; i--)
            {
                var behavior = behaviors[i];
                var nextDelegate = next;

                var behaviorMethod = behaviorType.GetMethod("HandleAsync");

                next = () => (Task<TResponse>)behaviorMethod.Invoke(behavior, new object[] { request, nextDelegate, cancellationToken });
            }

            return await next();
        }
    }
}