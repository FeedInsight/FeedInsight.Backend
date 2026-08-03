using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.ChatAssistant.DTOs;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Chats;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;

namespace FeedInsight.Application.Features.ChatAssistant.Commands.CreateChatSession;

public class CreateChatSessionCommandHandler
    : IRequestHandler<CreateChatSessionCommand, ErrorOr<ChatSessionDto>>
{
    private readonly IRepository<ChatSession> _chatSessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantResolver _tenantResolver;
    private readonly IUnitOfWork _unitOfWork;

    public CreateChatSessionCommandHandler(
        IRepository<ChatSession> chatSessionRepository,
        ICurrentUserService currentUserService,
        ITenantResolver tenantResolver,
        IUnitOfWork unitOfWork)
    {
        _chatSessionRepository = chatSessionRepository;
        _currentUserService = currentUserService;
        _tenantResolver = tenantResolver;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<ChatSessionDto>> HandleAsync(
        CreateChatSessionCommand request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated ||
            !_currentUserService.UserId.HasValue)
        {
            return Errors.Auth.Unauthenticated;
        }

        var tenantId = await _tenantResolver.ResolveTenantIdAsync(cancellationToken);

        if (!tenantId.HasValue)
        {
            return Errors.Tenants.NotFound;
        }

        var session = new ChatSession(
     tenantId.Value,
     _currentUserService.UserId.Value);

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            session.Rename(request.Title.Trim());
        }

        await _chatSessionRepository.AddAsync(session, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChatSessionDto(
            session.Id,
            session.Title,
            session.CreatedAt);
    }
}