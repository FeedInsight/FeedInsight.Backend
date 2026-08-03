using ErrorOr;
using FeedInsight.Application.Common.Interfaces;

using FeedInsight.Application.Features.ChatAssistant.DTOs;
using FeedInsight.Application.Features.ChatAssistant.Queries.GetChatSessions;
using FeedInsight.Application.Features.ChatAssistant.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Chats;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;

namespace FeedInsight.Application.Features.ChatAssistant.Queries.GetChatSessions;

public class GetChatSessionsQueryHandler
    : IRequestHandler<GetChatSessionsQuery, ErrorOr<IReadOnlyList<ChatSessionDto>>>
{
    private readonly IRepository<ChatSession> _chatRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantResolver _tenantResolver;

    public GetChatSessionsQueryHandler(
        IRepository<ChatSession> chatRepository,
        ICurrentUserService currentUserService,
        ITenantResolver tenantResolver)
    {
        _chatRepository = chatRepository;
        _currentUserService = currentUserService;
        _tenantResolver = tenantResolver;
    }

    public async Task<ErrorOr<IReadOnlyList<ChatSessionDto>>> HandleAsync(
        GetChatSessionsQuery request,
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

        var sessions = await _chatRepository.ListAsync(
            new ChatSessionLookupSpec(
                tenantId.Value,
                _currentUserService.UserId.Value),
            cancellationToken);

        return sessions
            .Select(x => new ChatSessionDto(
                x.Id,
                x.Title,
                x.CreatedAt))
            .ToList();
    }
}