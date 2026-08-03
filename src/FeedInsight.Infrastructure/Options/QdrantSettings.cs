namespace FeedInsight.Infrastructure.Options;

public class QdrantSettings
{
    public const string SectionName = "Qdrant";

    public string Url { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    // Maps CollectionName -> Its specific configuration
    public Dictionary<string, CollectionConfig> Collections { get; set; } = new();
}

public class CollectionConfig
{
    public ulong VectorSize { get; set; }
    
    // Default to Cosine as it's standard for text, but can be overridden
    public string Distance { get; set; } = "Cosine"; 
    
    public List<string> IndexFields { get; set; } = new();
}
