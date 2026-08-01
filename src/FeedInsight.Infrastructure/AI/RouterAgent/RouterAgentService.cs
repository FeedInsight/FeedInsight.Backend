using FeedInsight.Application.Features.AI.RouterAgent;
using FeedInsight.Application.Features.AI.RouterAgent.Models;
using FeedInsight.Domain.Categories;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FeedInsight.Infrastructure.AI.RouterAgent;

public class RouterAgentService : IRouterAgentService
{
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
        const string prompt = """
            You are an expert Product Owner AI Assistant.
            Your job is to analyze raw customer feedback, determine the overall sentiment, and extract discrete, actionable technical tasks.
            
            Available Categories (JSON format):
            {{$categories}}

            Rules:
            1. Analyze the raw feedback. If the customer mentions MULTIPLE distinct issues (e.g., a bug AND a feature request), you MUST split them into multiple separate JSON objects in the "tasks" array.
            2. EACH JSON object must represent exactly ONE technical intent. Do NOT use "and" or commas to combine intents in a single string.
            3. Translate emotional or vague language into clear, professional technical intents.
            4. Select the most appropriate CategoryId from the provided list. If none fit well, use the ID for the 'Uncategorized' category.
            5. Extract 3 to 5 comma-separated technical keywords for vector database indexing (e.g., "ui, accessibility, button").
            6. Determine the "overallSentiment" of the entire feedback. It MUST be exactly one of these three words: "Positive", "Neutral", or "Negative".

            Example Input:
            "The app is fast, but the profile picture upload crashes. Also, I really want a dark mode!"
            
            Example Output:
            {
              "overallSentiment": "Neutral",
              "tasks": [
                {
                  "extractedIntent": "Fix profile picture upload crash",
                  "categoryId": "00000000-0000-0000-0000-000000000000",
                  "technicalKeywords": "profile, upload, crash, bug"
                },
                {
                  "extractedIntent": "Implement dark mode feature",
                  "categoryId": "11111111-1111-1111-1111-111111111111",
                  "technicalKeywords": "dark mode, ui, theme, feature"
                }
              ]
            }

            Raw Customer Feedback:
            {{$feedback}}

            You MUST respond with a JSON object matching this exact schema:
            {
              "overallSentiment": "string",
              "tasks": [
                {
                  "extractedIntent": "string",
                  "categoryId": "guid",
                  "technicalKeywords": "string"
                }
              ]
            }
            """;

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

            var aiResponse = JsonSerializer.Deserialize<RouterAgentResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

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