using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.ExtractedTasks;
using FeedInsight.Domain.ExtractedTasks.Events;
using Microsoft.Extensions.Logging;

namespace FeedInsight.Infrastructure.Messaging.Handlers;

public class ExtractedTaskCreatedEventHandler : IDomainEventHandler<ExtractedTaskCreatedEvent>
{
    private readonly IRepository<ExtractedTask> _taskRepo;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDatabaseService _qdrantService;
    private readonly ILogger<ExtractedTaskCreatedEventHandler> _logger;

    public ExtractedTaskCreatedEventHandler(
        IRepository<ExtractedTask> taskRepo,
        IEmbeddingService embeddingService,
        IVectorDatabaseService qdrantService,
        ILogger<ExtractedTaskCreatedEventHandler> logger)
    {
        _taskRepo = taskRepo;
        _embeddingService = embeddingService;
        _qdrantService = qdrantService;
        _logger = logger;
    }

    public async Task HandleAsync(ExtractedTaskCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing ExtractedTaskCreatedEvent for TaskId: {TaskId}", domainEvent.ExtractedTaskId);

        var task = await _taskRepo.GetByIdAsync(domainEvent.ExtractedTaskId, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("ExtractedTask {TaskId} not found.", domainEvent.ExtractedTaskId);
            return;
        }

        var textToEmbed = $"{task.ExtractedIntent} {task.TechnicalKeywords}".Trim();
        
        _logger.LogInformation("Generating embedding for ExtractedTask {TaskId}", task.Id);
        var embedding = await _embeddingService.GenerateEmbeddingAsync(textToEmbed, cancellationToken);

        _logger.LogInformation("Upserting ExtractedTask {TaskId} to Qdrant", task.Id);
        await _qdrantService.UpsertTaskAsync(task, embedding, cancellationToken);
        
        _logger.LogInformation("Successfully saved ExtractedTask {TaskId} to Qdrant", task.Id);
    }
}
