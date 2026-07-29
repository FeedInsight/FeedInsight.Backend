using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Tenants.Commands.ConfigureJira
{
    public class ConfigureJiraCommandHandler : IRequestHandler<ConfigureJiraCommand, ErrorOr<Success>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IUnitOfWork _unitOfWork;        
        private readonly IEncryptor _encryptor;

        public ConfigureJiraCommandHandler(ICurrentUserService currentUserService, IRepository<User> userRepository, IRepository<Tenant> tenantRepository, IUnitOfWork unitOfWork, IEncryptor encryptor)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _unitOfWork = unitOfWork;
            _encryptor = encryptor;
        }

        public async Task<ErrorOr<Success>> HandleAsync(ConfigureJiraCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
            {
                return Errors.Auth.Unauthenticated;
            }

            var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
            if (user is null) return Errors.Users.NotFound;

            if (user.TenantId is null)
            {
                return Errors.Users.NotAssociatedWithTenant;
            }

            var tenant = await _tenantRepository.GetByIdAsync(user.TenantId.Value, cancellationToken);
            if (tenant is null) return Errors.Tenants.NotFound;

            tenant.ConfigureJira(request.JiraUrl, request.PersonalAccessToken, request.WebHookSecret, _encryptor);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}