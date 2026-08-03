using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Categories.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, ErrorOr<List<CategoryDto>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Category> _categoryRepository;

    public GetCategoriesQueryHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<Category> categoryRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ErrorOr<List<CategoryDto>>> HandleAsync(GetCategoriesQuery request, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null) return Errors.Auth.Unauthenticated;

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null || user.TenantId is null) return Errors.Users.NotAssociatedWithTenant;

        var categories = await _categoryRepository.ListAsync(new CategoriesByTenantSpec(user.TenantId.Value), cancellationToken);

        var dtos = categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.IsSystemDefault,
            c.CreatedAt
        )).OrderBy(c => c.Name).ToList();

        return dtos;
    }
}