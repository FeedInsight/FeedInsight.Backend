using Ardalis.Specification;
using FeedInsight.Domain.ExtractedTasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Specifications;

public class ExtractedTasksByFeedbackIdsSpec : Specification<ExtractedTask>
{
    public ExtractedTasksByFeedbackIdsSpec(List<Guid> feedbackIds)
    {
        Query.Where(x => feedbackIds.Contains(x.CustomerFeedbackId));
    }
}
