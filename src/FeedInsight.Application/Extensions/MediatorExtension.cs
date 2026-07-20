using FeedInsight.Application.Behaviors;
using FeedInsight.Application.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Application.Extensions;

public static class MediatorExtension
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        var applicationAssembly = typeof(IMediator).Assembly;

        var handlerTypes = applicationAssembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));

        foreach(var type in handlerTypes)
        {
            var interfaceType = type.GetInterfaces().First(i =>
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            services.AddTransient(interfaceType, type);
        }

        services.AddValidatorsFromAssembly(applicationAssembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
