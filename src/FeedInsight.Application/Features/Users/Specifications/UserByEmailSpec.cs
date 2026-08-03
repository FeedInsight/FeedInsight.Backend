using Ardalis.Specification;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Specifications;

public sealed class UserByEmailSpec : SingleResultSpecification<User>
{
    public UserByEmailSpec(string email)
    {
        Query.Where(u => u.Email == email)
             .Include(u => u.RefreshTokens)
             .Include(u => u.UserRoles)
             .ThenInclude(ur => ur.Role);
    }
}
