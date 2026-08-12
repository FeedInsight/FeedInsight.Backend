using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Events;
using FeedInsight.Domain.Categories;
using FeedInsight.Application.Features.Categories.Specifications;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

namespace FeedInsight.Infrastructure.Messaging.Handlers;

public class UserStoryEventHandler : 
    IDomainEventHandler<UserStoryCreatedEvent>,
    IDomainEventHandler<UserStoryUpdatedEvent>,
    IDomainEventHandler<UserStoryUrgencyIncreasedEvent>
{
    private readonly IRepository<UserStory> _userStoryRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDatabaseService _qdrantService;
    private readonly ILogger<UserStoryEventHandler> _logger;
    private readonly Kernel _kernel;

    public UserStoryEventHandler(
        IRepository<UserStory> userStoryRepo,
        IRepository<Category> categoryRepo,
        IEmbeddingService embeddingService,
        IVectorDatabaseService qdrantService,
        ILogger<UserStoryEventHandler> logger,
        Kernel kernel)
    {
        _userStoryRepo = userStoryRepo;
        _categoryRepo = categoryRepo;
        _embeddingService = embeddingService;
        _qdrantService = qdrantService;
        _logger = logger;
        _kernel = kernel;
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

        if (eventType == "Created" || eventType == "Updated")
        {
            await CategorizeUserStoryAsync(story, cancellationToken);
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

    private async Task CategorizeUserStoryAsync(UserStory story, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Evaluating category for UserStory: {StoryId}", story.Id);

        var categories = await _categoryRepo.ListAsync(new CategoriesByTenantSpec(story.TenantId), cancellationToken);
        if (!categories.Any()) return;

        var categoryContext = categories.Select(c => new
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        });

        string categoriesJson = JsonSerializer.Serialize(categoryContext);
        
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.2
        };

        var arguments = new KernelArguments(executionSettings)
        {
            { "categories", categoriesJson },
            { "title", story.Title },
            { "acceptanceCriteria", story.AcceptanceCriteria ?? "" }
        };

        try
        {
            var result = await _kernel.InvokePromptAsync(UserStoryCategorizationPrompts.SystemPrompt, arguments, cancellationToken: cancellationToken);
            string jsonResponse = result.GetValue<string>() ?? "{}";
            
            _logger.LogInformation("RAW AI RESPONSE FOR STORY {StoryId}: {Response}", story.Id, jsonResponse);

            // Clean markdown blocks if returned
            if (jsonResponse.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                jsonResponse = jsonResponse.Substring(7).TrimEnd('`').Trim();
            else if (jsonResponse.StartsWith("```", StringComparison.OrdinalIgnoreCase))
                jsonResponse = jsonResponse.Substring(3).TrimEnd('`').Trim();

            var responseDoc = JsonDocument.Parse(jsonResponse);
            if (responseDoc.RootElement.TryGetProperty("categoryId", out var categoryIdProp))
            {
                var idString = categoryIdProp.GetString();
                if (Guid.TryParse(idString, out Guid selectedCategoryId) && selectedCategoryId != Guid.Empty)
                {
                    // Only update if it actually changed
                    if (story.CategoryId != selectedCategoryId)
                    {
                        _logger.LogInformation("Categorized UserStory {StoryId} to Category {CategoryId}", story.Id, selectedCategoryId);
                        story.UpdateCategory(selectedCategoryId);
                        await _userStoryRepo.UpdateAsync(story, cancellationToken);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to auto-categorize UserStory {StoryId}", story.Id);
        }
    }
}
