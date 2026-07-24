namespace FeedInsight.Infrastructure.Security.Settings;

public class SecuritySettings
{
    public const string SectionName = "SecuritySettings";

    public string EncryptionKey { get; set; } = string.Empty;
}
