namespace FeedInsight.Application.Common.Options;

public class JiraSettings
{
    public const string SectionName = "JiraSettings";
    public string[] AllowedIssueTypes { get; set; } = Array.Empty<string>();
    public string DefaultProjectKey { get; set; } = string.Empty;
    public string? UrgencyCustomFieldId { get; set; }
    public List<JiraPriorityMapping> PriorityMappings { get; set; } = new();
}

public class JiraPriorityMapping
{
    public int MinUrgencyScore { get; set; }
    public string PriorityId { get; set; } = string.Empty;
}
