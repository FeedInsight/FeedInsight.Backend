using FeedInsight.Application.Features.CustomerFeedbacks.Queries.GetCustomerFeedbacks;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/feedbacks")]
public class CustomerFeedbacksController : ApiController
{
    private readonly IMediator _mediator;

    public CustomerFeedbacksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = Role.ProductOwner)]
    public async Task<IActionResult> GetFeedbacks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetCustomerFeedbacksQuery(page, pageSize);
        var result = await _mediator.SendAsync(query);

        return result.Match(
            paginatedResult => OkPaginated(
                data: paginatedResult.Items,
                currentPage: query.Page,
                pageSize: query.PageSize,
                totalItems: paginatedResult.TotalCount
            ),
            errors => Problem(errors)
        );
    }
}
