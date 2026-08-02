using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Chats;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;

namespace FeedInsight.Application.Features.ChatAssistant.Commands.RenameChatSession;

public class RenameChatSessionCommandHandler
    : IRequestHandler<RenameChatSessionCommand, ErrorOr<Success>>
{
    private readonly IRepository<ChatSession> _chatSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RenameChatSessionCommandHandler(
        IRepository<ChatSession> chatSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _chatSessionRepository = chatSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> HandleAsync(
        RenameChatSessionCommand request,
        CancellationToken cancellationToken = default)
    {
        var session = await _chatSessionRepository.GetByIdAsync(
            request.SessionId,
            cancellationToken);

        if (session is null)
        {
            return Errors.Chat.SessionNotFound;
        }

        session.Rename(request.Title);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}