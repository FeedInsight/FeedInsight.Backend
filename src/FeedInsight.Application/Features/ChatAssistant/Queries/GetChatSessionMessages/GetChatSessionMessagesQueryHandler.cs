using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.ChatAssistant.DTOs;
using FeedInsight.Application.Features.ChatAssistant.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Chats;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;

namespace FeedInsight.Application.Features.ChatAssistant.Queries.GetChatSessionMessages;

public class GetChatSessionMessagesQueryHandler
    : IRequestHandler<GetChatSessionMessagesQuery, ErrorOr<PaginatedResult<ChatMessageDto>>>
{
    private readonly IRepository<ChatMessage> _messageRepository;
    private readonly IRepository<ChatSession> _sessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantResolver _tenantResolver;


    public GetChatSessionMessagesQueryHandler(
        IRepository<ChatMessage> messageRepository,
        IRepository<ChatSession> sessionRepository,
        ICurrentUserService currentUserService,
        ITenantResolver tenantResolver)
    {
        _messageRepository = messageRepository;
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
        _tenantResolver = tenantResolver;
    }


    public async Task<ErrorOr<PaginatedResult<ChatMessageDto>>> HandleAsync(
        GetChatSessionMessagesQuery request,
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



       
        var messages = await _messageRepository.ListAsync(
            new ChatMessagesSpec(
                request.SessionId,
                request.Page,
                request.PageSize),
            cancellationToken);



        var items = messages
            .Select(x => new ChatMessageDto(
    x.Id,
    x.SenderRole.ToString(),
    x.Content,
    x.CreatedAt))
            .ToList();



       
        var totalCount = await _messageRepository.CountAsync(
            new ChatMessagesCountSpec(request.SessionId),
            cancellationToken);



        return new PaginatedResult<ChatMessageDto>(
            items,
            totalCount);
    }
}