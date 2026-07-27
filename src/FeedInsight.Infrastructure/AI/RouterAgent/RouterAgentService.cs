using FeedInsight.Application.Features.AI.RouterAgent;
using FeedInsight.Application.Features.AI.RouterAgent.Models;
using FeedInsight.Domain.Categories;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

namespace FeedInsight.Infrastructure.AI.RouterAgent;

internal class RouterAgentService : IRouterAgentService
{
    private readonly Kernel _kernel;
    private readonly ILogger<RouterAgentService> _logger;

    public RouterAgentService(Kernel kernel, ILogger<RouterAgentService> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    public async Task<List<ExtractedTaskResult>> ProcessFeedbackAsync(string rawFeedback, IEnumerable<Category> availableCategories, CancellationToken cancellationToken = default)
    {
        // 1. Prepare the needed data
        var categoryContext = availableCategories.Select(c => new
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        });

        string categoriesJson = JsonSerializer.Serialize(categoryContext);

        // 2. define the exact system prompt
        const string prompt = """
            You are an expert Product Owner AI Assistant.
            Your job is to analyze raw customer feedback and extract discrete, actionable technical tasks.
            
            Available Categories (JSON format):
            {{$categories}}

            Rules:
            1. Analyze the raw feedback. If it contains multiple distinct issues (e.g., a bug AND a feature request), split them into multiple separate tasks.
            2. If it is a single issue, output one task.
            3. Translate emotional or vague language into clear, professional technical intents (e.g., "The app is so slow when I click save" -> "Optimize performance of the save action").
            4. Select the most appropriate CategoryId from the provided list based on the category descriptions. If none fit well, use the ID for the 'Uncategorized' category.
            5. Extract 3 to 5 comma-separated technical keywords for vector database indexing (e.g., "ui, accessibility, button").

            Raw Customer Feedback:
            {{$feedback}}

            You MUST respond with a raw JSON array matching this exact schema:
            [
              {
                "extractedIntent": "string",
                "categoryId": "guid",
                "technicalKeywords": "string"
              }
            ]
            """;

        // 3. Configure the LLM to strictly return JSON
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.2 // Low temperature for high deterministic, analytical output
        };

        // 4. Pass the arguments to Semantic Kernel
        var arguments = new KernelArguments(executionSettings)
        {
            { "categories", categoriesJson },
            { "feedback", rawFeedback }
        };

        try
        {
            // 5. Execute the prompt
            var result = await _kernel.InvokePromptAsync(prompt, arguments, cancellationToken: cancellationToken);
            string jsonResponse = result.GetValue<string>() ?? "[]";

            // Note: Sometimes the LLM wraps the json_object response inside a single parent object like { "tasks": [...] }
            // If using pure array response format, we parse it directly. 
            // We use a small helper here to strip markdown blocks just in case the LLM hallucinates them.
            jsonResponse = CleanJsonOutput(jsonResponse);

            var extractedTasks = JsonSerializer.Deserialize<List<ExtractedTaskResult>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return extractedTasks ?? new List<ExtractedTaskResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process feedback through Router Agent LLM.");
            throw; // Let the background worker catch this and implement a retry policy
        }
    }

    /// <summary>
    /// Strips ```json and ``` markdown blocks if the LLM accidentally includes them.
    /// </summary>
    private static string CleanJsonOutput(string json)
    {
        json = json.Trim();
        if (json.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            json = json.Substring(7);
        }
        else if (json.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            json = json.Substring(3);
        }

        if (json.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            json = json.Substring(0, json.Length - 3);
        }

        return json.Trim();
    }
}
