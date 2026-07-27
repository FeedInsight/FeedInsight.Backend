using FeedInsight.Application.Features.Ingestion.Commands.SubmitFeedback;
using FeedInsight.Application.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/[controller]")]
public class IngestionController : ApiController
{
    private readonly IMediator _mediator;

    public IngestionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("feedback")]
    public async Task<IActionResult> SubmitFeedback([FromBody] SubmitFeedbackCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            id => OkResponse(new { FeedbackId = id, Message = "Feedback submitted successfully." }),
            errors => Problem(errors)
        );
    }
}
