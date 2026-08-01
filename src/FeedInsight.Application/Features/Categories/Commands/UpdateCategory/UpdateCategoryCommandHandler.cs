using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Categories.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, ErrorOr<Success>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
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

    public async Task<ErrorOr<Success>> HandleAsync(UpdateCategoryCommand request, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null) return Errors.Auth.Unauthenticated;

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null || user.TenantId is null) return Errors.Users.NotAssociatedWithTenant;

        var category = await _categoryRepository.SingleOrDefaultAsync(
            new CategoryByIdAndTenantSpec(request.Id, user.TenantId.Value), cancellationToken);

        if (category is null) return Errors.Categories.NotFound;

        if (category.IsSystemDefault) return Errors.Categories.CannotModifySystemDefault;

        var existingCategory = await _categoryRepository.SingleOrDefaultAsync(
            new CategoryByNameSpec(user.TenantId.Value, request.Name), cancellationToken);

        if (existingCategory is not null && existingCategory.Id != category.Id)
        {
            return Errors.Categories.DuplicateName;
        }

        category.UpdateDetails(request.Name, request.Description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}