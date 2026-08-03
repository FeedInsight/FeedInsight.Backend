using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.ChatAssistant.DTOs;
using FeedInsight.Application.Features.ChatAssistant.Queries.GetChatSessionById;
using FeedInsight.Application.Features.ChatAssistant.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Chats;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;

namespace FeedInsight.Application.Features.ChatAssistant.Queries.GetChatSessionById;

public class GetChatSessionByIdQueryHandler
    : IRequestHandler<GetChatSessionByIdQuery, ErrorOr<ChatSessionDetailsDto>>
{
    private readonly IRepository<ChatSession> _chatRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantResolver _tenantResolver;

    public GetChatSessionByIdQueryHandler(
        IRepository<ChatSession> chatRepository,
        ICurrentUserService currentUserService,
        ITenantResolver tenantResolver)
    {
        _chatRepository = chatRepository;
        _currentUserService = currentUserService;
        _tenantResolver = tenantResolver;
    }

    public async Task<ErrorOr<ChatSessionDetailsDto>> HandleAsync(
        GetChatSessionByIdQuery request,
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

        var session = await _chatRepository.FirstOrDefaultAsync(
            new ChatSessionByIdSpec(
                tenantId.Value,
                _currentUserService.UserId.Value,
                request.SessionId),
            cancellationToken);

        if (session is null)
        {
            return Errors.Chat.SessionNotFound;
        }

        return new ChatSessionDetailsDto(
     session.Id,
     session.Title,
     session.CreatedAt);
    }
}