using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Options;
using FeedInsight.Application.Features.Auth.Common;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;
using Microsoft.Extensions.Options;

namespace FeedInsight.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ErrorOr<AuthenticationResult>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenCommandHandler(
        IRepository<User> userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtOptions)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtOptions.Value;
    }
    public async Task<ErrorOr<AuthenticationResult>> HandleAsync(RefreshTokenCommand request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.SingleOrDefaultAsync(new UserByRefreshTokenSpec(request.RefreshToken), cancellationToken);

        if (user is null || user.IsLocked)
        {
            return Errors.Auth.SessionExpired;
        }

        // 2. Validate the specific token
        var currentToken = user.RefreshTokens.SingleOrDefault(rt => rt.Token == request.RefreshToken);

        if (currentToken is null || !currentToken.IsActive)
        {
            return Errors.Auth.SessionExpired;
        }

        // token rotation
        currentToken.Revoke();

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        var newRefreshToken = new FeedInsight.Domain.Users.RefreshToken(
            user.Id,
            newRefreshTokenString,
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays));

        user.AddRefreshToken(newRefreshToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        int expiresInSeconds = _jwtSettings.ExpiryMinutes * 60;

        return new AuthenticationResult(
            user.Id,
            user.FirstName,
            user.LastName,
            newAccessToken,
            newRefreshTokenString,
            expiresInSeconds
        );
    }
}
