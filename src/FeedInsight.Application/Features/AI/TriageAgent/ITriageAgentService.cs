using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FeedInsight.Application.Features.AI.TriageAgent.Models;
using FeedInsight.Domain.ExtractedTasks;

namespace FeedInsight.Application.Features.AI.TriageAgent;

public interface ITriageAgentService
{
    Task<TriageAgentResponse> ProcessClusterAsync(
        IReadOnlyList<ExtractedTask> clusterTasks,
        CancellationToken cancellationToken = default);
}
