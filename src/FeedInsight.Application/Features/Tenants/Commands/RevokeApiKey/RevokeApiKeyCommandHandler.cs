using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Tenants.Commands.RevokeApiKey;

public class RevokeApiKeyCommandHandler : IRequestHandler<RevokeApiKeyCommand, ErrorOr<Success>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<ApiKey> _apiKeyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeApiKeyCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<ApiKey> apiKeyRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _apiKeyRepository = apiKeyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> HandleAsync(RevokeApiKeyCommand request, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null) return Errors.Auth.Unauthenticated;

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null || user.TenantId is null) return Errors.Users.NotAssociatedWithTenant;

        var apiKey = await _apiKeyRepository.GetByIdAsync(request.ApiKeyId, cancellationToken);

        if (apiKey is null) return Errors.ApiKeys.NotFound;

        if (apiKey.TenantId != user.TenantId.Value) return Errors.ApiKeys.NotFound;

        apiKey.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}