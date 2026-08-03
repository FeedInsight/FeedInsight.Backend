using System.Text.Json;
using FeedInsight.Application.Features.AI.RouterAgent;
using FeedInsight.Application.Features.AI.RouterAgent.Models;
using FeedInsight.Domain.Categories;
using FeedInsight.Infrastructure.AI.RouterAgent.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace FeedInsight.Infrastructure.AI.RouterAgent;

public class RouterAgentService : IRouterAgentService
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Kernel _kernel;
    private readonly ILogger<RouterAgentService> _logger;

    public RouterAgentService(Kernel kernel, ILogger<RouterAgentService> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    public async Task<RouterAgentResponse> ProcessFeedbackAsync(
        string rawFeedback,
        IEnumerable<Category> availableCategories,
        CancellationToken cancellationToken = default)
    {
        // 1. Prepare the dynamic categories for the prompt
        // We only pass the Id, Name, and Description so the LLM doesn't waste tokens on internal DB fields
        var categoryContext = availableCategories.Select(c => new
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        });

        string categoriesJson = JsonSerializer.Serialize(categoryContext);

        // 2. define the exact system prompt
        string prompt = RouterAgentPrompts.SystemPrompt;

        // 3. Configure the LLM to strictly return JSON
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            // Note: If Hugging Face throws a 400 Bad Request error about "response_format", 
            // simply comment this line out. Llama 3.1 is smart enough to follow the prompt without it!
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
            string jsonResponse = result.GetValue<string>() ?? "{}";

            // LOG THE RAW OUTPUT: This will print exactly what the LLM generated to your terminal
            _logger.LogInformation("Raw LLM Response: \n{Response}", jsonResponse);

            // Clean and format the JSON dynamically based on what the LLM decided to output
            jsonResponse = CleanJsonOutput(jsonResponse);

            var aiResponse = JsonSerializer.Deserialize<RouterAgentResponse>(jsonResponse, _jsonSerializerOptions);

            return aiResponse ?? new RouterAgentResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process feedback through Router Agent LLM.");
            throw; // Let the background worker catch this and implement a retry policy
        }
    }

    /// <summary>
    /// Aggressively extracts and formats the JSON, handling edge cases where the LLM 
    /// is chatty or ignores the wrapper object schema.
    /// </summary>
    private static string CleanJsonOutput(string response)
    {
        if (string.IsNullOrWhiteSpace(response)) return "{}";

        response = response.Trim();

        // 1. Strip Markdown formatting if the LLM hallucinates it
        if (response.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            response = response.Substring(7);
        else if (response.StartsWith("```", StringComparison.OrdinalIgnoreCase))
            response = response.Substring(3);

        if (response.EndsWith("```", StringComparison.OrdinalIgnoreCase))
            response = response.Substring(0, response.Length - 3);

        response = response.Trim();

        // 2. Ideal Case: It followed instructions and returned the Parent Object
        if (response.StartsWith("{") && response.EndsWith("}"))
        {
            return response;
        }

        // 3. Stubborn Case: It returned a raw Array of tasks instead of the wrapper object.
        // We will manually wrap it to prevent a deserialization crash!
        if (response.StartsWith("[") && response.EndsWith("]"))
        {
            return $"{{\"overallSentiment\": \"Neutral\", \"tasks\": {response}}}";
        }

        // 4. Bruteforce Fallback: Search for the bounds of the JSON object inside conversational text
        int objStart = response.IndexOf('{');
        int objEnd = response.LastIndexOf('}');
        if (objStart != -1 && objEnd != -1 && objEnd > objStart)
        {
            return response.Substring(objStart, objEnd - objStart + 1);
        }

        // 5. Bruteforce Fallback 2: Search for an array inside conversational text and wrap it
        int arrStart = response.IndexOf('[');
        int arrEnd = response.LastIndexOf(']');
        if (arrStart != -1 && arrEnd != -1 && arrEnd > arrStart)
        {
            string arrayOnly = response.Substring(arrStart, arrEnd - arrStart + 1);
            return $"{{\"overallSentiment\": \"Neutral\", \"tasks\": {arrayOnly}}}";
        }

        return "{}";
    }
}