using FeedInsight.Application.Features.Analytics.Queries.GetLatestAnalyticsSnapshot;
using FeedInsight.Application.Features.Analytics.Queries.GetAnalyticsSnapshots;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = Role.ProductOwner)]
public class AnalyticsController : ApiController
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("snapshots/latest")]
    public async Task<IActionResult> GetLatestSnapshot()
    {
        var query = new GetLatestAnalyticsSnapshotQuery();

        var result = await _mediator.SendAsync(query);

        return result.Match(
            snapshot => OkResponse(snapshot),
            errors => Problem(errors));
    }

    [HttpGet("snapshots")]
    public async Task<IActionResult> GetSnapshots(
        [FromQuery] GetAnalyticsSnapshotsQuery query)
    {
        var result = await _mediator.SendAsync(query);

        return result.Match(
            paginatedResult => OkPaginated(
                data: paginatedResult.Items,
                currentPage: query.Page,
                pageSize: query.PageSize,
                totalItems: paginatedResult.TotalCount),
            errors => Problem(errors));
    }
}