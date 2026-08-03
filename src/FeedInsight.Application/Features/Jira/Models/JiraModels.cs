using System.Text.Json.Serialization;

namespace FeedInsight.Application.Features.Jira.Models;

public class JiraWebhookPayloadDto
{
    [JsonPropertyName("webhookEvent")]
    public string WebhookEvent { get; set; } = string.Empty;

    [JsonPropertyName("issue")]
    public JiraIssueDto Issue { get; set; } = new();
}

public class JiraSearchResponseDto
{
    [JsonPropertyName("issues")]
    public List<JiraIssueDto> Issues { get; set; } = new();

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}

public class JiraIssueDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("fields")]
    public JiraIssueFieldsDto Fields { get; set; } = new();
}

public class JiraIssueFieldsDto
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public JiraStatusDto Status { get; set; } = new();

    [JsonPropertyName("issuetype")]
    public JiraIssueTypeDto Issuetype { get; set; } = new();
}

public class JiraStatusDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class JiraIssueTypeDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
