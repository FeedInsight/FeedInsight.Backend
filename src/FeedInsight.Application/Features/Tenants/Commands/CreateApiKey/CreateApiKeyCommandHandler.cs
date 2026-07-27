using System.Security.Cryptography;
using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Tenants.Commands.CreateApiKey;

public class CreateApiKeyCommandHandler : IRequestHandler<CreateApiKeyCommand, ErrorOr<string>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApiKeyHasher _apiKeyHasher;
    private readonly IApiKeyGenerator _apiKeyGenerator;

    public CreateApiKeyCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<Tenant> tenantRepository,
        IUnitOfWork unitOfWork,
        IApiKeyHasher apiKeyHasher,
        IApiKeyGenerator apiKeyGenerator
        )
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _apiKeyHasher = apiKeyHasher;
        _apiKeyGenerator = apiKeyGenerator;
    }

    public async Task<ErrorOr<string>> HandleAsync(CreateApiKeyCommand request, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null)
        {
            return Errors.Auth.Unauthenticated;
        }

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null) return Errors.Users.NotFound;

        if (user.TenantId is null)
        {
            return Errors.Users.NotAssociatedWithTenant;
        }

        var tenant = await _tenantRepository.GetByIdAsync(user.TenantId.Value, cancellationToken);
        if (tenant is null) return Errors.Tenants.NotFound;

        var plainTextKey = _apiKeyGenerator.Generate();
        var expiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddYears(1);

        var apiKey = new ApiKey(tenant.Id, request.Name, plainTextKey, expiresAt, _apiKeyHasher);

        tenant.AddApiKey(apiKey);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return plainTextKey;
    }
}
