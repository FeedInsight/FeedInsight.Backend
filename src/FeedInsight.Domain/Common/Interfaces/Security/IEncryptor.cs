namespace FeedInsight.Domain.Common.Interfaces.Security;

public interface IEncryptor
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
