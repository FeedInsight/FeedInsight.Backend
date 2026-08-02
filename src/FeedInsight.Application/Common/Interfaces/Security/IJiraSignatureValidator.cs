namespace FeedInsight.Application.Common.Interfaces.Security;

public interface IJiraSignatureValidator
{
    bool IsSignatureValid(string payload, string secret, string signatureHeader);
}
