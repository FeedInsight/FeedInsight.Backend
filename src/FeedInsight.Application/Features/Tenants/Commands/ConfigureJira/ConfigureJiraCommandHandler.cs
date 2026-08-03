using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Users;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Application.Features.Tenants.Commands.ConfigureJira
{
    public class ConfigureJiraCommandHandler : IRequestHandler<ConfigureJiraCommand, ErrorOr<Success>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IUnitOfWork _unitOfWork;        
        private readonly IEncryptor _encryptor;
        private readonly IServiceScopeFactory _scopeFactory;

        public ConfigureJiraCommandHandler(ICurrentUserService currentUserService, IRepository<User> userRepository, IRepository<Tenant> tenantRepository, IUnitOfWork unitOfWork, IEncryptor encryptor, IServiceScopeFactory scopeFactory)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _unitOfWork = unitOfWork;
            _encryptor = encryptor;
            _scopeFactory = scopeFactory;
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

            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var jiraSyncService = scope.ServiceProvider.GetRequiredService<IJiraSyncService>();

                try
                {
                    // Use CancellationToken.None so the sync isn't cancelled when the user's HTTP request finishes!
                    await jiraSyncService.TriggerInitialBulkSyncAsync(tenant.Id, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Background Jira sync failed for Tenant {tenant.Id}: {ex.Message}");
                }
            });

            return Result.Success;
        }
    }
}