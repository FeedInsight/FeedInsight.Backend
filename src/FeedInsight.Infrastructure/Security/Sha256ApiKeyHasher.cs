using FeedInsight.Domain.Common.Interfaces.Security;
using System.Security.Cryptography;
using System.Text;

namespace FeedInsight.Infrastructure.Security;

public class Sha256ApiKeyHasher : IApiKeyHasher
{
    public string Hash(string plainTextKey)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(plainTextKey);
        var hashBytes = sha256.ComputeHash(bytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
