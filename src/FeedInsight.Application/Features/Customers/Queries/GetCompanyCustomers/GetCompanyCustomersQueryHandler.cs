using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.Customers.Specifications;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Customers.Queries.GetCompanyCustomers;

public class GetCompanyCustomersQueryHandler : IRequestHandler<GetCompanyCustomersQuery, ErrorOr<PaginatedResult<CompanyCustomerDto>>>
{
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCompanyCustomersQueryHandler(
        IRepository<User> userRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<PaginatedResult<CompanyCustomerDto>>> HandleAsync(
        GetCompanyCustomersQuery request,
        CancellationToken cancellationToken = default)
    {
       
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            return Errors.Auth.InvalidCredentials;
        }

       
        var currentUser = await _userRepository.FirstOrDefaultAsync(
            new UserByIdSpec(currentUserId.Value),
            cancellationToken);

        if (currentUser is null || currentUser.TenantId is null)
        {
            return Errors.Auth.InvalidCredentials;
        }

        
        var customers = await _userRepository.ListAsync(
            new CompanyCustomersSpec(currentUser.TenantId.Value),
            cancellationToken);

        
        var items = customers
            .Select(user => new CompanyCustomerDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.IsLocked,
                user.CreatedAt))
            .ToList();

        return new PaginatedResult<CompanyCustomerDto>(
            items,
            items.Count);
    }
}