using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Ingestion.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.Tenants;

namespace FeedInsight.Application.Features.Ingestion.Commands.SubmitFeedback;

public class SubmitFeedbackCommandHandler : IRequestHandler<SubmitFeedbackCommand, ErrorOr<Guid>>
{
    private readonly ICurrentApiKeyService _currentApiKeyService;
    private readonly IApiKeyHasher _apiKeyHasher;
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IRepository<CustomerFeedback> _feedbackRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitFeedbackCommandHandler(
        ICurrentApiKeyService currentApiKeyService,
        IApiKeyHasher apiKeyHasher,
        IRepository<Tenant> tenantRepository,
        IRepository<CustomerFeedback> feedbackRepository,
        IUnitOfWork unitOfWork)
    {
        _currentApiKeyService = currentApiKeyService;
        _apiKeyHasher = apiKeyHasher;
        _tenantRepository = tenantRepository;
        _feedbackRepository = feedbackRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> HandleAsync(SubmitFeedbackCommand request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_currentApiKeyService.ApiKey))
        {
            return Errors.Auth.MissingApiKey;
        }

        var keyHash = _apiKeyHasher.Hash(_currentApiKeyService.ApiKey);

        var spec = new TenantByApiKeyHashSpec(keyHash);
        var tenant = await _tenantRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (tenant is null)
        {
            return Errors.Auth.InvalidApiKey;
        }

        var apiKey = tenant.ApiKeys.FirstOrDefault(k => k.KeyHash == keyHash);
        if (apiKey is null || !apiKey.IsActive)
        {
            return Errors.Auth.InvalidApiKey;
        }

        var feedback = new CustomerFeedback(
            tenant.Id,
            request.RawContent,
            request.SubmitterEmail,
            request.MetadataJson);

        await _feedbackRepository.AddAsync(feedback, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return feedback.Id;
    }
}
