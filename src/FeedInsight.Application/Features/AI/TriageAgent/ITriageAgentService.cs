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

    Task<DeduplicationDecisionResponse> DetermineDeduplicationAsync(
        TriageAgentResponse draftStory,
        IReadOnlyList<FeedInsight.Application.Common.Models.VectorSearchResult<FeedInsight.Application.Common.Models.UserStoryPayload>> candidateStories,
        CancellationToken cancellationToken = default);
}
