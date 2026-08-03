using Azure.Core;
using FeedInsight.Application.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/webhooks/jira")]
[AllowAnonymous] // Jira doesn't have a JWT, it uses HMAC signatures
public class JiraWebhooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public JiraWebhooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Receives real-time issue updates from Jira (e.g., ticket moved to 'Done', title changed).
    /// </summary>
    [HttpPost("{tenantId:guid}")]
    public async Task<IActionResult> ReceiveJiraEvent(Guid tenantId)
    {
        // 1. Read the raw body (required for HMAC signature validation)
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        // 2. Fetch the signature Jira put in the header
        if (!Request.Headers.TryGetValue("X-Hub-Signature", out var signatureHeader))
        {
            return Unauthorized("Missing Jira Signature Header.");
        }

        // 3. Dispatch the secure MediatR command to handle the HMAC validation and update
        var command = new FeedInsight.Application.Features.Jira.Commands.ProcessJiraWebhook.ProcessJiraWebhookCommand(
            tenantId,
            signatureHeader!,
            payload
        );

        var result = await _mediator.SendAsync(command);

        // 4. Always return Ok() (even on failure) so Jira doesn't aggressively retry Webhooks
        return Ok();
    }
}