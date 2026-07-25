using Ardalis.Specification;
using FeedInsight.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Users.Specifications;

public sealed class RoleByNameSpec : SingleResultSpecification<Role>
{
    public RoleByNameSpec(string name)
    {
        Query.AsNoTracking()
             .Where(r => r.Name == name);
    }
}
