using System.Text.Json;
using FeedInsight.Application.Features.AI.TriageAgent;
using FeedInsight.Application.Features.AI.TriageAgent.Models;
using FeedInsight.Domain.ExtractedTasks;
using FeedInsight.Infrastructure.AI.TriageAgent.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace FeedInsight.Infrastructure.AI.TriageAgent;

public class TriageAgentService : ITriageAgentService
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Kernel _kernel;
    private readonly ILogger<TriageAgentService> _logger;

    public TriageAgentService(Kernel kernel, ILogger<TriageAgentService> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    public async Task<TriageAgentResponse> ProcessClusterAsync(
        IReadOnlyList<ExtractedTask> clusterTasks,
        CancellationToken cancellationToken = default)
    {
        if (clusterTasks == null || !clusterTasks.Any())
        {
            return new TriageAgentResponse();
        }

        // 1. Prepare tasks for the prompt
        var tasksContext = clusterTasks.Select(t => new
        {
            t.Id,
            t.ExtractedIntent,
            t.TechnicalKeywords
        });

        string tasksJson = JsonSerializer.Serialize(tasksContext);

        // 2. define the exact system prompt
        string prompt = TriageAgentPrompts.SystemPrompt;

        // 3. Configure the LLM to strictly return JSON
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.2 // Low temperature for consistent generation
        };

        // 4. Pass the arguments to Semantic Kernel
        var arguments = new KernelArguments(executionSettings)
        {
            { "tasks", tasksJson }
        };

        try
        {
            // 5. Execute the prompt
            var result = await _kernel.InvokePromptAsync(prompt, arguments, cancellationToken: cancellationToken);
            string jsonResponse = result.GetValue<string>() ?? "{}";

            _logger.LogInformation("Triage Agent LLM Response: \n{Response}", jsonResponse);

            jsonResponse = CleanJsonOutput(jsonResponse);

            var aiResponse = JsonSerializer.Deserialize<TriageAgentResponse>(jsonResponse, _jsonSerializerOptions);

            return aiResponse ?? new TriageAgentResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process task cluster through Triage Agent LLM.");
            throw;
        }
    }

    private static string CleanJsonOutput(string response)
    {
        if (string.IsNullOrWhiteSpace(response)) return "{}";

        response = response.Trim();

        if (response.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            response = response.Substring(7);
        else if (response.StartsWith("```", StringComparison.OrdinalIgnoreCase))
            response = response.Substring(3);

        if (response.EndsWith("```", StringComparison.OrdinalIgnoreCase))
            response = response.Substring(0, response.Length - 3);

        response = response.Trim();

        if (response.StartsWith("{") && response.EndsWith("}"))
        {
            return response;
        }

        int objStart = response.IndexOf('{');
        int objEnd = response.LastIndexOf('}');
        if (objStart != -1 && objEnd != -1 && objEnd > objStart)
        {
            return response.Substring(objStart, objEnd - objStart + 1);
        }

        return "{}";
    }

    public async Task<DeduplicationDecisionResponse> DetermineDeduplicationAsync(
        TriageAgentResponse draftStory,
        IReadOnlyList<FeedInsight.Application.Common.Models.VectorSearchResult<FeedInsight.Application.Common.Models.UserStoryPayload>> candidateStories,
        CancellationToken cancellationToken = default)
    {
        if (candidateStories == null || !candidateStories.Any())
        {
            return new DeduplicationDecisionResponse { IsDuplicate = false };
        }

        var candidateContext = candidateStories.Select(s => new
        {
            StoryId = s.PointId,
            Title = s.Payload?.Title,
            AcceptanceCriteria = s.Payload?.AcceptanceCriteria
        });

        string draftStoryJson = JsonSerializer.Serialize(draftStory, _jsonSerializerOptions);
        string candidatesJson = JsonSerializer.Serialize(candidateContext, _jsonSerializerOptions);

        string prompt = TriageAgentPrompts.DeduplicationPrompt;

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.1 
        };

        var arguments = new KernelArguments(executionSettings)
        {
            { "draftStory", draftStoryJson },
            { "candidateStories", candidatesJson }
        };

        try
        {
            var result = await _kernel.InvokePromptAsync(prompt, arguments, cancellationToken: cancellationToken);
            string jsonResponse = result.GetValue<string>() ?? "{}";

            _logger.LogInformation("Triage Agent Deduplication Response: \n{Response}", jsonResponse);

            jsonResponse = CleanJsonOutput(jsonResponse);

            var aiResponse = JsonSerializer.Deserialize<DeduplicationDecisionResponse>(jsonResponse, _jsonSerializerOptions);

            return aiResponse ?? new DeduplicationDecisionResponse { IsDuplicate = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process deduplication decision through Triage Agent LLM.");
            // Fallback to false so we don't drop data, it will just create a new story
            return new DeduplicationDecisionResponse { IsDuplicate = false };
        }
    }
}
