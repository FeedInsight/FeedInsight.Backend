using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Options;
using FeedInsight.Application.Features.Auth.Common;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Tenants.Enums;
using FeedInsight.Domain.Users;
using Microsoft.Extensions.Options;

namespace FeedInsight.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<AuthenticationResult>>
{
    private readonly IRepository<User> _userRepository;
    private readonly ITenantStatusChecker _tenantStatusChecker;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public LoginCommandHandler(
        IRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtOptions, ITenantStatusChecker tenantStatusChecker)
    {

        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _tenantStatusChecker = tenantStatusChecker;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtOptions.Value;
       
    }
    public async Task<ErrorOr<AuthenticationResult>> HandleAsync(LoginCommand request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FirstOrDefaultAsync(new UserByEmailSpec(request.Email), cancellationToken);

        if (user is null)
        {
            return Errors.Auth.InvalidCredentials;
        }

        if (user.IsLocked)
        {
            return Errors.Auth.AccountLocked;
        }

        if (!user.VerifyPassword(request.Password, _passwordHasher))
        {
            return Errors.Auth.InvalidCredentials;
        }
        // Check Tenant
        if (user.TenantId.HasValue)
        {
            var tenantStatusResult = await _tenantStatusChecker.EnsureActiveAsync(
                user.TenantId.Value,
                cancellationToken);

            if (tenantStatusResult.IsError)
            {
                return tenantStatusResult.Errors;
            }
        }

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshToken = new FeedInsight.Domain.Users.RefreshToken(user.Id, refreshTokenString, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays));
        user.AddRefreshToken(refreshToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        int expiresInSeconds = _jwtSettings.ExpiryMinutes * 60;

        return new AuthenticationResult(
            user.Id,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshTokenString,
            expiresInSeconds
        );
    }
}