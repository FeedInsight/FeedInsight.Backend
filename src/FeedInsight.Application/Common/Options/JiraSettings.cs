namespace FeedInsight.Application.Common.Options;

public class JiraSettings
{
    public const string SectionName = "JiraSettings";
    public string[] AllowedIssueTypes { get; set; } = Array.Empty<string>();
    public string DefaultProjectKey { get; set; } = string.Empty;
}
