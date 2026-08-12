using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.AI.Clustering;
using FeedInsight.Application.Features.ExtractedTasks.Specifications;
using FeedInsight.Domain.ExtractedTasks;
using FeedInsight.Infrastructure.Options;
using MathNet.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeedInsight.Infrastructure.AI.Clustering;

public class TaskClusteringService : ITaskClusteringService
{
    private readonly IVectorDatabaseService _vectorDatabaseService;
    private readonly IRepository<ExtractedTask> _taskRepository;
    private readonly DbScanSettings _dbScanSettings;
    private readonly ILogger<TaskClusteringService> _logger;

    public TaskClusteringService(
        IVectorDatabaseService vectorDatabaseService,
        IRepository<ExtractedTask> taskRepository,
        IOptions<DbScanSettings> dbScanSettings,
        ILogger<TaskClusteringService> logger)
    {
        _vectorDatabaseService = vectorDatabaseService;
        _taskRepository = taskRepository;
        _dbScanSettings = dbScanSettings.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<List<ExtractedTask>>> ClusterTenantTasksAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting clustering for Tenant {TenantId}", tenantId);

        // 1. Fetch unassigned tasks from SQL
        var spec = new UnassignedExtractedTasksByTenantSpec(tenantId);
        var unassignedTasks = await _taskRepository.ListAsync(spec, cancellationToken);

        if (!unassignedTasks.Any())
        {
            _logger.LogInformation("No unassigned tasks found for Tenant {TenantId}", tenantId);
            return Array.Empty<List<ExtractedTask>>();
        }

        var taskIds = unassignedTasks.Select(t => t.Id).ToList();

        // 2. Fetch corresponding embeddings from Qdrant
        var qdrantPoints = await _vectorDatabaseService.RetrievePointsAsync<ExtractedTaskPayload>(
            "extracted_tasks", 
            taskIds, 
            cancellationToken);

        // 3. Match SQL entities to embeddings
        var points = new List<DbscanPoint>();
        foreach (var task in unassignedTasks)
        {
            var point = qdrantPoints.FirstOrDefault(p => p.Id == task.Id);
            if (point != null)
            {
                points.Add(new DbscanPoint { Task = task, Embedding = point.Embedding.ToArray() });
            }
            else
            {
                _logger.LogWarning("Embedding not found in Qdrant for Task {TaskId}", task.Id);
            }
        }

        if (points.Count == 0)
        {
            return Array.Empty<List<ExtractedTask>>();
        }

        // 4. Run Custom DBSCAN for High-Dimensional data using Cosine Distance
        _logger.LogInformation("Running custom DBSCAN clustering on {Count} points. Epsilon: {Epsilon}, MinPts: {MinPts}", 
            points.Count, _dbScanSettings.Epsilon, _dbScanSettings.MinimumPointsPerCluster);

        var clusters = CalculateClusters(points, _dbScanSettings.Epsilon, _dbScanSettings.MinimumPointsPerCluster);

        _logger.LogInformation("Clustering completed. Found {ClusterCount} clusters.", clusters.Count);

        return clusters;
    }

    private List<List<ExtractedTask>> CalculateClusters(List<DbscanPoint> points, double epsilon, int minPts)
    {
        var clusters = new List<List<ExtractedTask>>();
        int clusterId = 0;

        foreach (var p in points)
        {
            if (p.IsVisited) continue;
            p.IsVisited = true;

            var neighbors = GetRegion(points, p, epsilon);
            if (neighbors.Count < minPts)
            {
                p.IsNoise = true;
            }
            else
            {
                clusterId++;
                var cluster = new List<ExtractedTask>();
                clusters.Add(cluster);
                ExpandCluster(points, p, neighbors, cluster, epsilon, minPts);
            }
        }

        return clusters;
    }

    private void ExpandCluster(
        List<DbscanPoint> points, 
        DbscanPoint p, 
        List<DbscanPoint> neighbors, 
        List<ExtractedTask> cluster, 
        double epsilon, 
        int minPts)
    {
        p.IsClustered = true;
        cluster.Add(p.Task);

        // Using a queue for BFS expansion
        var queue = new Queue<DbscanPoint>(neighbors);
        
        while (queue.Count > 0)
        {
            var currentPoint = queue.Dequeue();

            if (!currentPoint.IsVisited)
            {
                currentPoint.IsVisited = true;
                var currentNeighbors = GetRegion(points, currentPoint, epsilon);
                if (currentNeighbors.Count >= minPts)
                {
                    foreach (var neighbor in currentNeighbors)
                    {
                        // Add only if not already in queue to avoid duplicates in processing
                        if (!neighbor.IsVisited && !queue.Contains(neighbor)) 
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            if (!currentPoint.IsClustered)
            {
                currentPoint.IsClustered = true;
                currentPoint.IsNoise = false;
                cluster.Add(currentPoint.Task);
            }
        }
    }

    private List<DbscanPoint> GetRegion(List<DbscanPoint> points, DbscanPoint p, double epsilon)
    {
        var region = new List<DbscanPoint>();
        foreach (var other in points)
        {
            // Calculate cosine distance (1 - cosine similarity)
            double distance = Distance.Cosine(p.Embedding, other.Embedding);
            if (distance <= epsilon)
            {
                region.Add(other);
            }
        }
        return region;
    }

    private class DbscanPoint
    {
        public ExtractedTask Task { get; set; } = null!;
        public float[] Embedding { get; set; } = null!;
        public bool IsVisited { get; set; }
        public bool IsClustered { get; set; }
        public bool IsNoise { get; set; }
    }
}
