using System.Security.Cryptography;
using System.Text;
using FeedInsight.Application.Common.Interfaces.Security;

namespace FeedInsight.Infrastructure.Security;

public class JiraSignatureValidator : IJiraSignatureValidator
{
    public bool IsSignatureValid(string payload, string secret, string signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader) || !signatureHeader.StartsWith("sha256="))
        {
            return false;
        }

        string providedHash = signatureHeader.Substring(7);

        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(secretBytes);
        var computedHashBytes = hmac.ComputeHash(payloadBytes);
        var computedHashString = Convert.ToHexString(computedHashBytes).ToLowerInvariant();

        return providedHash == computedHashString;
    }
}
