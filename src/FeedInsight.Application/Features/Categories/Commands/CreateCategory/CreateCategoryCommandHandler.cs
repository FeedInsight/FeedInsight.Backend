using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Categories.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, ErrorOr<Guid>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> HandleAsync(CreateCategoryCommand request, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null) return Errors.Auth.Unauthenticated;

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null || user.TenantId is null) return Errors.Users.NotAssociatedWithTenant;

        var exists = await _categoryRepository.AnyAsync(new CategoryByNameSpec(user.TenantId.Value, request.Name), cancellationToken);
        if (exists) return Errors.Categories.DuplicateName;

        var category = new Category(
            tenantId: user.TenantId.Value,
            name: request.Name,
            description: request.Description
        );

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}