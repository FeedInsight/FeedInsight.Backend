using Ardalis.Specification;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Specifications;

public sealed class UserByIdSpec : SingleResultSpecification<User>
{
    public UserByIdSpec(Guid userId)
    {
        Query.Where(u => u.Id == userId)
             .Include(u => u.UserRoles)
             .ThenInclude(ur => ur.Role);
    }
}
