using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Customers.Commands.LockCompanyCustomer
{
    public class LockCompanyCustomerCommandHandler: IRequestHandler<LockCompanyCustomerCommand, ErrorOr<Success>>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LockCompanyCustomerCommandHandler(
            IRepository<User> userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<Success>> HandleAsync(
            LockCompanyCustomerCommand request,
            CancellationToken cancellationToken = default)
        {
            var customer = await _userRepository.SingleOrDefaultAsync(
                new UserByIdSpec(request.CustomerId),
                cancellationToken);

            if (customer is null)
                return Errors.Users.NotFound;

            if (!customer.HasRole(Role.CompanyCustomer))
                return Errors.Users.NotCustomer;

            customer.LockAccount(request.Reason);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
