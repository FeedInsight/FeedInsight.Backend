using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Events;
using Microsoft.Extensions.Logging;

namespace FeedInsight.Infrastructure.Messaging.Handlers;

public class UserStoryEventHandler : 
    IDomainEventHandler<UserStoryCreatedEvent>,
    IDomainEventHandler<UserStoryUpdatedEvent>,
    IDomainEventHandler<UserStoryUrgencyIncreasedEvent>
{
    private readonly IRepository<UserStory> _userStoryRepo;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDatabaseService _qdrantService;
    private readonly ILogger<UserStoryEventHandler> _logger;

    public UserStoryEventHandler(
        IRepository<UserStory> userStoryRepo,
        IEmbeddingService embeddingService,
        IVectorDatabaseService qdrantService,
        ILogger<UserStoryEventHandler> logger)
    {
        _userStoryRepo = userStoryRepo;
        _embeddingService = embeddingService;
        _qdrantService = qdrantService;
        _logger = logger;
    }

    public Task HandleAsync(UserStoryCreatedEvent domainEvent, CancellationToken cancellationToken)
        => ProcessEventAsync(domainEvent.UserStoryId, "Created", cancellationToken);

    public Task HandleAsync(UserStoryUpdatedEvent domainEvent, CancellationToken cancellationToken)
        => ProcessEventAsync(domainEvent.UserStoryId, "Updated", cancellationToken);

    public Task HandleAsync(UserStoryUrgencyIncreasedEvent domainEvent, CancellationToken cancellationToken)
        => ProcessEventAsync(domainEvent.UserStoryId, "UrgencyIncreased", cancellationToken);

    private async Task ProcessEventAsync(Guid userStoryId, string eventType, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing UserStory{EventType}Event for UserStoryId: {UserStoryId}", eventType, userStoryId);

        var story = await _userStoryRepo.GetByIdAsync(userStoryId, cancellationToken);
        if (story == null)
        {
            _logger.LogWarning("UserStory {UserStoryId} not found.", userStoryId);
            return;
        }

        var textToEmbed = $"{story.Title} {story.AcceptanceCriteria}".Trim();

        _logger.LogInformation("Generating embedding for UserStory {UserStoryId}", story.Id);
        var embedding = await _embeddingService.GenerateEmbeddingAsync(textToEmbed, cancellationToken);

        _logger.LogInformation("Upserting UserStory {UserStoryId} to Qdrant", story.Id);
        
        var payload = new UserStoryPayload(
            story.TenantId,
            story.CategoryId,
            story.Source.ToString(),
            story.JiraTicketKey,
            story.Title,
            story.AcceptanceCriteria,
            story.UrgencyScore,
            story.Status.ToString(),
            textToEmbed
        );

        await _qdrantService.UpsertPointAsync("user_stories", story.Id, embedding, payload, cancellationToken);

        _logger.LogInformation("Successfully saved UserStory {UserStoryId} to Qdrant", story.Id);
    }
}
