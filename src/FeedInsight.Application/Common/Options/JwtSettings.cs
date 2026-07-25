namespace FeedInsight.Application.Common.Options;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; } 
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}