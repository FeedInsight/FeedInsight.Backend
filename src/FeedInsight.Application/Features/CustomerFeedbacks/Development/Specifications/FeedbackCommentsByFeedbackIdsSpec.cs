using Ardalis.Specification;
using FeedInsight.Domain.CustomerFeedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Development.Specifications
{
    public class FeedbackCommentsByFeedbackIdsSpec : Specification<FeedbackComment>
    {
        public FeedbackCommentsByFeedbackIdsSpec(List<Guid> feedbackIds)
        {
            Query.Where(c => feedbackIds.Contains(c.CustomerFeedbackId))
                 .OrderBy(c => c.CreatedAt);
        }
    }
}
