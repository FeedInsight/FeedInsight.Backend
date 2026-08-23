namespace FeedInsight.Infrastructure.Options;

public class HuggingFaceSettings
{
    public const string SectionName = "HuggingFace";

    public string ApiKey { get; set; } = string.Empty;
    public string[] EmbeddingApiKeys { get; set; } = [];
    public string ChatModelId { get; set; } = "meta-llama/Llama-3.1-8B-Instruct";
    public string ChatEndpoint { get; set; } = "https://router.huggingface.co/v1/";
    public string EmbeddingModelId { get; set; } = "qwen3-embedding-8b";
    public string EmbeddingEndpoint { get; set; } = "https://router.huggingface.co/scaleway/v1/";
}
