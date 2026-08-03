using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.ChatAssistant.DTOs;
using FeedInsight.Application.Features.ChatAssistant.Queries.AskProductAssistant;
using FeedInsight.Application.Features.ChatAssistant.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Chats;
using FeedInsight.Domain.Chats.Enums;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;

namespace FeedInsight.Application.Features.ChatAssistant.Commands.SendChatMessage;

public class SendChatMessageCommandHandler
    : IRequestHandler<SendChatMessageCommand, ErrorOr<SendMessageResponseDto>>
{
    private readonly IRepository<ChatSession> _sessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantResolver _tenantResolver;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;


    public SendChatMessageCommandHandler(
        IRepository<ChatSession> sessionRepository,
        ICurrentUserService currentUserService,
        ITenantResolver tenantResolver,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
        _tenantResolver = tenantResolver;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }


    public async Task<ErrorOr<SendMessageResponseDto>> HandleAsync(
        SendChatMessageCommand request,
        CancellationToken cancellationToken = default)
    {

        if (!_currentUserService.IsAuthenticated ||
            !_currentUserService.UserId.HasValue)
        {
            return Errors.Auth.Unauthenticated;
        }


        var tenantId =
            await _tenantResolver.ResolveTenantIdAsync(cancellationToken);


        if (!tenantId.HasValue)
        {
            return Errors.Tenants.NotFound;
        }



        var session = await _sessionRepository.FirstOrDefaultAsync(
            new ChatSessionByIdSpec(
                tenantId.Value,
                _currentUserService.UserId.Value,
                request.SessionId),
            cancellationToken);



        if (session is null)
        {
            return Errors.Chat.SessionNotFound;
        }



        // Save user message
        session.AddMessage(
            ChatRole.User,
            request.Content);



        var userMessage = session.Messages.Last();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var assistantResult = await _mediator.SendAsync(
            new AskProductAssistantQuery(request.SessionId, request.Content),
            cancellationToken);

        if (assistantResult.IsError)
        {
            return assistantResult.Errors;
        }

        session.AddMessage(
            ChatRole.Assistant,
            assistantResult.Value);



        var assistantMessage = session.Messages.Last();



        await _unitOfWork.SaveChangesAsync(cancellationToken);



        return new SendMessageResponseDto(

            new ChatMessageDto(
                userMessage.Id,
                userMessage.SenderRole.ToString(),
                userMessage.Content,
                userMessage.CreatedAt),


            new ChatMessageDto(
                assistantMessage.Id,
                assistantMessage.SenderRole.ToString(),
                assistantMessage.Content,
                assistantMessage.CreatedAt)
        );
    }
}