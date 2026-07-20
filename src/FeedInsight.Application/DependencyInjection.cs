using FeedInsight.Application.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // register custom mediator
        services.AddMediator();
        // services.AddTransient<IRequestHandler<CreateFeedCommand, Guid>, CreateFeedHandler>();

        return services;
    }
}
