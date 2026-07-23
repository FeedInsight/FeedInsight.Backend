using FeedInsight.Application.Features.Feeds.Commands.CreateFeed;
using FeedInsight.Application.Features.Feeds.Queries.GetFeeds;
using FeedInsight.Application.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers
{
    [Route("api/[controller]")]
    public class FeedsController : ApiController
    {
        private readonly IMediator _mediator;

        public FeedsController(IMediator mediator)
        {
            _mediator = mediator;
        }


        /// <summary>
        /// GET: api/feeds?searchTerm=xyz&page=1&pageSize=10
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFeeds([FromQuery] GetFeedsQuery query)
        {
            // 1. Send the query to the Application Layer
            var result = await _mediator.SendAsync(query);

            // 2. Match the ErrorOr result
            return result.Match(
                // Success: Use the OkPaginated helper from ApiController
                paginatedResult => OkPaginated(
                    data: paginatedResult.Items,
                    currentPage: query.Page,
                    pageSize: query.PageSize,
                    totalItems: paginatedResult.TotalCount
                ),

                // Failure: Use the Problem helper from ApiController
                errors => Problem(errors)
            );
        }


        [HttpPost]
        public async Task<IActionResult> CreateFeed([FromBody] CreateFeedCommand command)
        {
            var result = await _mediator.SendAsync(command);

            return result.Match(
                value => OkResponse(value), // Automatically wraps it in { data: ... }
                errors => Problem(errors)   // Automatically formats validations or errors
            );
        }
    }
}
