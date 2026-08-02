using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.AI.RouterAgent;
using FeedInsight.Application.Features.Categories.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.CustomerFeedbacks.Events;
using FeedInsight.Domain.ExtractedTasks;
using Microsoft.Extensions.Logging;

namespace FeedInsight.Infrastructure.Messaging.Handlers;

public class CustomerFeedbackCreatedEventHandler : IDomainEventHandler<CustomerFeedbackCreatedEvent>
{
    private readonly IRepository<CustomerFeedback> _feedbackRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<ExtractedTask> _extractedTaskRepo;
    private readonly IRouterAgentService _routerAgent;
    private readonly ILogger<CustomerFeedbackCreatedEventHandler> _logger;

    public CustomerFeedbackCreatedEventHandler(
        IRepository<CustomerFeedback> feedbackRepo,
        IRepository<Category> categoryRepo,
        IRepository<ExtractedTask> extractedTaskRepo,
        IRouterAgentService routerAgent,
        ILogger<CustomerFeedbackCreatedEventHandler> logger)
    {
        _feedbackRepo = feedbackRepo;
        _categoryRepo = categoryRepo;
        _extractedTaskRepo = extractedTaskRepo;
        _routerAgent = routerAgent;
        _logger = logger;
    }

    public async Task HandleAsync(CustomerFeedbackCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing CustomerFeedbackCreatedEvent for FeedbackId: {FeedbackId}", domainEvent.CustomerFeedbackId);

        var feedback = await _feedbackRepo.GetByIdAsync(domainEvent.CustomerFeedbackId, cancellationToken);
        if (feedback == null)
        {
            _logger.LogWarning("Feedback {FeedbackId} not found.", domainEvent.CustomerFeedbackId);
            return;
        }

        if (feedback.IsProcessedByRouter)
        {
            _logger.LogInformation("Feedback {FeedbackId} is already processed. Skipping.", domainEvent.CustomerFeedbackId);
            return;
        }

        // Fetch categories for this tenant
        var categories = await _categoryRepo.ListAsync(new CategoriesByTenantSpec(domainEvent.TenantId), cancellationToken);
        var fallbackCategory = categories.FirstOrDefault(c => c.IsSystemDefault);

        // Process via AI router
        var aiResponse = await _routerAgent.ProcessFeedbackAsync(feedback.RawContent, categories, cancellationToken);

        foreach (var aiTask in aiResponse.Tasks)
        {
            var validCategoryId = categories.Any(c => c.Id == aiTask.CategoryId)
                ? aiTask.CategoryId
                : (fallbackCategory?.Id ?? Guid.Empty);

            if (validCategoryId == Guid.Empty) continue;

            var extractedTask = new ExtractedTask(
                tenantId: domainEvent.TenantId,
                customerFeedbackId: feedback.Id,
                categoryId: validCategoryId,
                extractedIntent: aiTask.ExtractedIntent,
                technicalKeywords: aiTask.TechnicalKeywords
            );

            await _extractedTaskRepo.AddAsync(extractedTask, cancellationToken);
        }

        feedback.MarkAsProcessed(aiResponse.OverallSentiment);
        await _feedbackRepo.UpdateAsync(feedback, cancellationToken);
        
        _logger.LogInformation("Successfully processed FeedbackId: {FeedbackId}", domainEvent.CustomerFeedbackId);
    }
}
