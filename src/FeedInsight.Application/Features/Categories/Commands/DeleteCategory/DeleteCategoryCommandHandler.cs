using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Categories.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, ErrorOr<Success>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
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

    public async Task<ErrorOr<Success>> HandleAsync(DeleteCategoryCommand request, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null) return Errors.Auth.Unauthenticated;

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null || user.TenantId is null) return Errors.Users.NotAssociatedWithTenant;

        var category = await _categoryRepository.SingleOrDefaultAsync(
            new CategoryByIdAndTenantSpec(request.Id, user.TenantId.Value), cancellationToken);

        if (category is null) return Errors.Categories.NotFound;

        if (category.IsSystemDefault) return Errors.Categories.CannotModifySystemDefault;

        category.SoftDelete();

        // Note: Extracted Tasks assigned to this category will still exist. 
        // A background job or Domain Event should ideally migrate them to 'Uncategorized' 
        // to prevent orphaned vectors!

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}