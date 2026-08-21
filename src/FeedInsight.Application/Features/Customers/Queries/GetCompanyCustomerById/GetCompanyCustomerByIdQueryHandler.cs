using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Customers.Queries.GetCompanyCustomerById;

public class GetCompanyCustomerByIdQueryHandler
    : IRequestHandler<GetCompanyCustomerByIdQuery, ErrorOr<CompanyCustomerDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;

    public GetCompanyCustomerByIdQueryHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<CompanyCustomerDto>> HandleAsync(
        GetCompanyCustomerByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;

        if (!currentUserId.HasValue)
        {
            return Errors.Auth.Unauthenticated;
        }

        var currentUser = await _userRepository.FirstOrDefaultAsync(
            new UserByIdSpec(currentUserId.Value),
            cancellationToken);

        if (currentUser is null || !currentUser.TenantId.HasValue)
        {
            return Errors.Auth.Unauthenticated;
        }

        var customer = await _userRepository.FirstOrDefaultAsync(
            new UserByIdSpec(request.CustomerId),
            cancellationToken);

        if (customer is null ||
            customer.TenantId != currentUser.TenantId)
        {
            return Errors.Users.NotFound;
        }

        if (!customer.HasRole(Role.CompanyCustomer))
        {
            return Errors.Users.NotFound;
        }

        return new CompanyCustomerDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.IsLocked);
    }
}