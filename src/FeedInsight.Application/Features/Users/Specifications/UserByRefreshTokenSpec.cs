using Ardalis.Specification;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Specifications;

public sealed class UserByRefreshTokenSpec : SingleResultSpecification<User>
{
    public UserByRefreshTokenSpec(string refreshToken)
    {
        Query.Where(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken))
             .Include(u => u.RefreshTokens)
             .Include(u => u.UserRoles)
             .ThenInclude(ur => ur.Role);
    }
}
