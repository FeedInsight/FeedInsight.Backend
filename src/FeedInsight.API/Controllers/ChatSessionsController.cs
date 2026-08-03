using FeedInsight.Application.Features.ChatAssistant.Commands.CreateChatSession;
using FeedInsight.Application.Features.ChatAssistant.Commands.DeleteChatSession;
using FeedInsight.Application.Features.ChatAssistant.Commands.RenameChatSession;
using FeedInsight.Application.Features.ChatAssistant.Commands.SendChatMessage;
using FeedInsight.Application.Features.ChatAssistant.Queries.GetChatSessionById;
using FeedInsight.Application.Features.ChatAssistant.Queries.GetChatSessionMessages;
using FeedInsight.Application.Features.ChatAssistant.Queries.GetChatSessions;
using FeedInsight.Application.Messaging;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ChatSessionsController : ApiController
{
    private readonly IMediator _mediator;

    public ChatSessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpPost]
    public async Task<IActionResult> CreateSession(
        [FromBody] CreateChatSessionCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            session => OkResponse(session),
            errors => Problem(errors));
    }



    [HttpGet]
    public async Task<IActionResult> GetSessions()
    {
        var result = await _mediator.SendAsync(
            new GetChatSessionsQuery());

        return result.Match(
            sessions => OkResponse(sessions),
            errors => Problem(errors));
    }



    [HttpGet("{sessionId:guid}")]
    public async Task<IActionResult> GetSessionById(
        Guid sessionId)
    {
        var result = await _mediator.SendAsync(
            new GetChatSessionByIdQuery(sessionId));

        return result.Match(
            session => OkResponse(session),
            errors => Problem(errors));
    }



    [HttpPatch("{sessionId:guid}")]
    public async Task<IActionResult> RenameSession(
        Guid sessionId,
        [FromBody] RenameChatSessionRequest request)
    {
        var command = new RenameChatSessionCommand(
            sessionId,
            request.Title);

        var result = await _mediator.SendAsync(command);

        return result.Match(
            _ => OkResponse(new
            {
                Message = "Chat session updated successfully."
            }),
            errors => Problem(errors));
    }



    [HttpDelete("{sessionId:guid}")]
    public async Task<IActionResult> DeleteSession(
        Guid sessionId)
    {
        var result = await _mediator.SendAsync(
            new DeleteChatSessionCommand(sessionId));

        return result.Match(
            _ => OkResponse(new
            {
                Message = "Chat session deleted successfully."
            }),
            errors => Problem(errors));
    }



    [HttpGet("{sessionId:guid}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid sessionId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await _mediator.SendAsync(
            new GetChatSessionMessagesQuery(
                sessionId,
                page,
                pageSize));

        return result.Match(
            messages => OkPaginated(
                data: messages.Items,
                currentPage: page ?? 1,
                pageSize: pageSize ?? 20,
                totalItems: messages.TotalCount),
            errors => Problem(errors));
    }



    [HttpPost("{sessionId:guid}/messages")]
    public async Task<IActionResult> SendMessage(
        Guid sessionId,
        [FromBody] SendMessageRequest request)
    {
        var command = new SendChatMessageCommand(
            sessionId,
            request.Content);

        var result = await _mediator.SendAsync(command);

        return result.Match(
            response => OkResponse(response),
            errors => Problem(errors));
    }
}