using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Models;
using FeedInsight.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace FeedInsight.Infrastructure.BackgroundJobs;

public class OutboxBackgroundProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxBackgroundProcessor> _logger;

    public OutboxBackgroundProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxBackgroundProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Background Processor is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                List<OutboxMessage> messages;
                
                // Fetch batch using a brief scope
                using (var fetchScope = _scopeFactory.CreateScope())
                {
                    var fetchDbContext = fetchScope.ServiceProvider.GetRequiredService<FeedInsightDbContext>();
                    messages = await fetchDbContext.OutboxMessages
                        .Where(m => m.ProcessedOnUtc == null)
                        .Take(20)
                        .ToListAsync(stoppingToken);
                }

                if (messages.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                // Process each message in its own isolated scope
                foreach (var message in messages)
                {
                    using var processScope = _scopeFactory.CreateScope();
                    var dbContext = processScope.ServiceProvider.GetRequiredService<FeedInsightDbContext>();
                    var dispatcher = processScope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

                    // Fetch the tracked entity for this specific isolated context
                    var trackedMessage = await dbContext.OutboxMessages.FindAsync(new object[] { message.Id }, stoppingToken);
                    
                    if (trackedMessage == null || trackedMessage.ProcessedOnUtc != null) continue;

                    try
                    {
                        var domainEvent = DeserializeDomainEvent(trackedMessage);

                        if (domainEvent != null)
                        {
                            await dispatcher.DispatchAsync(domainEvent, stoppingToken);
                        }

                        trackedMessage.ProcessedOnUtc = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing outbox message {MessageId}", trackedMessage.Id);
                        trackedMessage.Error = ex.ToString();
                        trackedMessage.ProcessedOnUtc = DateTime.UtcNow; // Processed but failed
                    }

                    // Save changes for this specific message and its handler side-effects
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in Outbox Background Processor. Retrying in 5 seconds.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("Outbox Background Processor is stopping.");
    }

    private IDomainEvent? DeserializeDomainEvent(OutboxMessage message)
    {
        // Try to find the event type in the Domain assembly (assuming all domain events are there)
        var assembly = typeof(IDomainEvent).Assembly;
        
        var eventType = assembly.GetTypes().FirstOrDefault(t => t.Name == message.Type);

        if (eventType == null)
        {
            _logger.LogWarning("Domain Event type {TypeName} not found.", message.Type);
            return null;
        }

        var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as IDomainEvent;
        return domainEvent;
    }
}
