using FeedInsight.Application.Messaging;
using FeedInsight.Domain.CustomerFeedbacks.Events;
using FeedInsight.Domain.ExtractedTasks.Events;
using FeedInsight.Domain.UserStories.Events;
using FeedInsight.Infrastructure.BackgroundJobs;
using FeedInsight.Infrastructure.Messaging;
using FeedInsight.Infrastructure.Messaging.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Infrastructure.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator>();
        
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddHostedService<OutboxBackgroundProcessor>();

        services.AddScoped<IDomainEventHandler<CustomerFeedbackCreatedEvent>, CustomerFeedbackCreatedEventHandler>();
        services.AddScoped<IDomainEventHandler<ExtractedTaskCreatedEvent>, ExtractedTaskCreatedEventHandler>();
        
        services.AddScoped<IDomainEventHandler<UserStoryCreatedEvent>, UserStoryEventHandler>();
        services.AddScoped<IDomainEventHandler<UserStoryUpdatedEvent>, UserStoryEventHandler>();
        services.AddScoped<IDomainEventHandler<UserStoryUrgencyIncreasedEvent>, UserStoryEventHandler>();
        
        return services;
    }
}
