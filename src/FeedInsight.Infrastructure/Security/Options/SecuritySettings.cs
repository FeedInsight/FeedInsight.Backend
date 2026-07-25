namespace FeedInsight.Infrastructure.Security.Options;

public class SecuritySettings
{
    public const string SectionName = "SecuritySettings";

    public string EncryptionKey { get; set; } = string.Empty;
}
