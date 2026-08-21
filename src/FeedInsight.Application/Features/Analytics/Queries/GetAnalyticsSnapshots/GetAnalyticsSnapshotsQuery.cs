using ErrorOr;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.Analytics.DTOs;
using FeedInsight.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Analytics.Queries.GetAnalyticsSnapshots
{
    public record GetAnalyticsSnapshotsQuery(
     DateOnly? From,
     DateOnly? To,
     int Page = 1,
     int PageSize = 20)
     : IRequest<ErrorOr<PaginatedResult<AnalyticsSnapshotDto>>>;
}
