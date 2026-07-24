using FeedInsight.Domain.Common.Interfaces.Security;
using System.Security.Cryptography;

namespace FeedInsight.Infrastructure.Security;

public class SecureApiKeyGenerator : IApiKeyGenerator
{
    private const string Prefix = "fi_live_";
    private const int KeyLengthInBytes = 32;
    public string Generate()
    {
        var randomBytes = new byte[KeyLengthInBytes];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var KeyContent = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        return $"{Prefix}{KeyContent}";
    }
}
