using FeedInsight.Application.Common.Interfaces;
using Microsoft.Extensions.Primitives;

namespace FeedInsight.API.Services;

public class CurrentApiKeyService : ICurrentApiKeyService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentApiKeyService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? ApiKey
    {
        get
        {
            if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-Api-Key", out StringValues apiKeyHeader) == true)
            {
                return apiKeyHeader.FirstOrDefault();
            }

            return null;
        }
    }
}
