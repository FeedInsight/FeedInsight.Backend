using Ardalis.Specification;
using FeedInsight.Domain.CustomerFeedbacks;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Specifications;

public sealed class UnprocessedFeedbackSpec : Specification<CustomerFeedback>
{
    public UnprocessedFeedbackSpec(int batchSize = 10)
    {
        Query.Where(cf => !cf.IsProcessedByRouter)
             .OrderBy(cf => cf.CreatedAt)
             .Take(batchSize);
    }
}