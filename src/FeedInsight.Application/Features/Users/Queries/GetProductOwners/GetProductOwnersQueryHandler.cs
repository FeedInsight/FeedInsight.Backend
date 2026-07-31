using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Queries.GetProductOwners;

public class GetProductOwnersQueryHandler : IRequestHandler<GetProductOwnersQuery, ErrorOr<PaginatedResult<ProductOwnerDto>>>
{
    private readonly IRepository<User> _userRepository;

    public GetProductOwnersQueryHandler(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<PaginatedResult<ProductOwnerDto>>> HandleAsync(GetProductOwnersQuery request, CancellationToken cancellationToken = default)
    {
        var totalCount = await _userRepository.CountAsync(
            new ProductOwnersSpec(request.SearchTerm, request.TenantId, request.IsActive),
            cancellationToken);

        var users = await _userRepository.ListAsync(
            new ProductOwnersSpec(request.SearchTerm, request.TenantId, request.IsActive, request.Page, request.PageSize),
            cancellationToken);

        var items = users
            .Select(u => new ProductOwnerDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Tenant != null ? u.Tenant.CompanyName : "Unknown",
                u.IsLocked,
                u.CreatedAt))
            .ToList();

        return new PaginatedResult<ProductOwnerDto>(items, totalCount);
    }
}
