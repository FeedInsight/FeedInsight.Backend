using FeedInsight.Application.Common.Interfaces;
using System.Security.Claims;

namespace FeedInsight.API.Services;

/// <summary>
/// Extracts user information from the active HTTP Context (e.g., from a JWT token).
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
    }

    public bool IsAuthenticated => UserId.HasValue;
}
