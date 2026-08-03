using FeedInsight.Application.Features.AI.ProductAssistant;
using FeedInsight.Application.Features.AI.ProductAssistant.Models;
using FeedInsight.Infrastructure.AI.ProductAssistant.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace FeedInsight.Infrastructure.AI.ProductAssistant;

public class ProductAssistantService : IProductAssistantService
{
    private const int SlidingWindowSize = 6;

    private readonly Kernel _kernel;
    private readonly ILogger<ProductAssistantService> _logger;

    public ProductAssistantService(Kernel kernel, ILogger<ProductAssistantService> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    public async Task<string> GenerateAnswerAsync(
        string userQuestion,
        string databaseContext,
        IReadOnlyList<ChatHistoryMessage> conversationHistory,
        CancellationToken cancellationToken = default)
    {
        var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();

        var systemPrompt = ProductAssistantPrompts.SystemPromptTemplate
            .Replace("{{$context}}", databaseContext);

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(systemPrompt);

        var recentMessages = conversationHistory
            .TakeLast(SlidingWindowSize)
            .ToList();

        foreach (var message in recentMessages)
        {
            if (string.Equals(message.Role, "User", StringComparison.OrdinalIgnoreCase))
            {
                chatHistory.AddUserMessage(message.Content);
            }
            else if (string.Equals(message.Role, "Assistant", StringComparison.OrdinalIgnoreCase))
            {
                chatHistory.AddAssistantMessage(message.Content);
            }
        }

        chatHistory.AddUserMessage(userQuestion);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0.3
        };

        try
        {
            var response = await chatCompletion.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                _kernel,
                cancellationToken);

            var content = response.Content ?? string.Empty;

            _logger.LogInformation("Product Assistant generated response of length {Length}", content.Length);

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Product Assistant response.");
            throw;
        }
    }
}
