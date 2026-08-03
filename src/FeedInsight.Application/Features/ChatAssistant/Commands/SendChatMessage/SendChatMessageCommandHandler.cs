using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.ChatAssistant.DTOs;
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


    public SendChatMessageCommandHandler(
        IRepository<ChatSession> sessionRepository,
        ICurrentUserService currentUserService,
        ITenantResolver tenantResolver,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
        _tenantResolver = tenantResolver;
        _unitOfWork = unitOfWork;
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



        // Temporary dummy AI response
        var assistantContent =
            "There are 3 open stories referencing payment failures: Story-101, Story-102, Story-103.";



        // Save assistant message
        session.AddMessage(
            ChatRole.Assistant,
            assistantContent);



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