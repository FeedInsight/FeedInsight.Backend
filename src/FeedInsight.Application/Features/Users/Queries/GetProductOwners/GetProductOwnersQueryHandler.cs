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
        var countSpec = new ProductOwnersFilterSpec(request.SearchTerm, request.TenantId);
        var totalCount = await _userRepository.CountAsync(countSpec, cancellationToken);

        var listSpec = new ProductOwnersPaginatedSpec(request.SearchTerm, request.Page, request.PageSize);
        var items = await _userRepository.ListAsync(listSpec, cancellationToken);

        return new PaginatedResult<ProductOwnerDto>(items, totalCount);
    }
}
