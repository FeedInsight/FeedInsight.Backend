namespace FeedInsight.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    // You can add more properties later, like:
    // string? Email { get; }
    // bool IsAdmin { get; }
}
