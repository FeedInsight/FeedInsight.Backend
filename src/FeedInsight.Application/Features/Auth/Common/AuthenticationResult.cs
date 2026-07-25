namespace FeedInsight.Application.Features.Auth.Common;

public record AuthenticationResult(
    Guid UserId,
    string FirstName,
    string LastName,
    string AccessToken,
    string RefreshToken,
    int ExpiresIn // Seconds until the access token expires
);