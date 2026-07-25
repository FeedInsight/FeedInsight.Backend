using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Infrastructure.Security.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace FeedInsight.Infrastructure.Security;

public class AesEncryptor : IEncryptor
{
    private readonly byte[] _encryptionKey;

    public AesEncryptor(IOptions<SecuritySettings> options)
    {
        var key = options.Value.EncryptionKey;

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("EncryptionKey is missing from SecuritySettings configuration.");
        }

        _encryptionKey = Convert.FromBase64String(key);
    }
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream();

        memoryStream.Write(aes.IV, 0, aes.IV.Length);

        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        using (var streamWriter = new StreamWriter(cryptoStream))
        {
            streamWriter.Write(plainText);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        // Extract the IV from the beginning of the cipher text
        var iv = new byte[aes.IV.Length];
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);

        return streamReader.ReadToEnd();
    }

}
