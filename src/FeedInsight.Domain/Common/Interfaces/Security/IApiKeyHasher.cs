namespace FeedInsight.Domain.Common.Interfaces.Security;

public interface IApiKeyHasher
{
    string Hash(string plainTextKey);
}
