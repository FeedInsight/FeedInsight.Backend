using FeedInsight.Application.Features.Feeds.Commands.CreateFeed;
using FeedInsight.Application.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedsController : ApiController
    {
        private readonly IMediator _mediator;

        public FeedsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeed([FromBody] CreateFeedCommand command)
        {
            var result = await _mediator.SendAsync(command);

            return result.Match(
                value => Ok(value), // Success
                errors => Problem(errors) // Automatically maps errors to 400/404/500
            );
        }
    }
}
