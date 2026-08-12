namespace FeedInsight.Application.Features.AI.Clustering.Models;

public class TenantClusteringResult
{
    public int TotalClustersProcessed { get; set; }
    public List<ClusterProcessResult> Results { get; set; } = new();
}

public class ClusterProcessResult
{
    public string Action { get; set; } = string.Empty;
    public Guid? UserStoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public float? Score { get; set; }
    public int TasksClustered { get; set; }
    public string? Reason { get; set; }
}
