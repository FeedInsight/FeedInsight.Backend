using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.AI.RouterAgent;
using FeedInsight.Application.Features.Categories.Specifications;
using FeedInsight.Application.Features.CustomerFeedbacks.Specifications;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.ExtractedTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeedInsight.Infrastructure.BackgroundJobs;

public class RouterBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RouterBackgroundService> _logger;

    public RouterBackgroundService(IServiceScopeFactory scopeFactory, ILogger<RouterBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Router Background Service is starting.");

        // Keep looping as long as the application is running
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1. Create a fresh DI scope for this iteration
                using var scope = _scopeFactory.CreateScope();

                var feedbackRepo = scope.ServiceProvider.GetRequiredService<IRepository<CustomerFeedback>>();
                var categoryRepo = scope.ServiceProvider.GetRequiredService<IRepository<Category>>();
                var extractedTaskRepo = scope.ServiceProvider.GetRequiredService<IRepository<ExtractedTask>>();
                var routerAgent = scope.ServiceProvider.GetRequiredService<IRouterAgentService>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // 2. Fetch the next batch of unprocessed feedback
                var unprocessedFeedbacks = await feedbackRepo.ListAsync(new UnprocessedFeedbackSpec(batchSize: 10), stoppingToken);

                if (unprocessedFeedbacks.Count == 0)
                {
                    // If the queue is empty, sleep for 5 seconds before checking again to save CPU
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                _logger.LogInformation("Processing {Count} new feedback items.", unprocessedFeedbacks.Count);

                foreach (var feedback in unprocessedFeedbacks)
                {
                    // 3. Fetch the specific categories for this customer's tenant
                    var categories = await categoryRepo.ListAsync(new CategoriesByTenantSpec(feedback.TenantId), stoppingToken);

                    // Find the fallback category just in case the AI hallucinates a bad ID
                    var fallbackCategory = categories.FirstOrDefault(c => c.IsSystemDefault);

                    // 4. Send the raw text and valid categories to the LLM
                    var aiResults = await routerAgent.ProcessFeedbackAsync(feedback.RawContent, categories, stoppingToken);

                    // 5. Convert the raw JSON DTOs from the LLM into Domain Entities
                    foreach (var aiTask in aiResults)
                    {
                        // Validation: Ensure the LLM didn't invent a fake CategoryId
                        var validCategoryId = categories.Any(c => c.Id == aiTask.CategoryId)
                            ? aiTask.CategoryId
                            : (fallbackCategory?.Id ?? Guid.Empty);

                        // If there is somehow no fallback, skip to prevent a database constraint crash
                        if (validCategoryId == Guid.Empty) continue;

                        var extractedTask = new ExtractedTask(
                            tenantId: feedback.TenantId,
                            customerFeedbackId: feedback.Id,
                            categoryId: validCategoryId,
                            extractedIntent: aiTask.ExtractedIntent,
                            technicalKeywords: aiTask.TechnicalKeywords
                        );

                        await extractedTaskRepo.AddAsync(extractedTask, stoppingToken);
                    }

                    // 6. Mark the original feedback as processed 
                    // Note: We are setting sentiment to "Unknown" for now. We can update the LLM prompt 
                    // later to return Sentiment analysis alongside the tasks!
                    feedback.MarkAsProcessed("Unknown");
                }

                // 7. Commit the entire batch to the database in one secure transaction
                await unitOfWork.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Successfully processed batch and saved Extracted Tasks.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the Router Background Service. Retrying in 10 seconds.");

                // If the OpenAI API goes down or the DB transiently fails, back off for 10 seconds before retrying
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("Router Background Service is stopping.");
    }
}
