using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestSearchController : ControllerBase
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDatabaseService _vectorDatabaseService;

    public TestSearchController(IEmbeddingService embeddingService, IVectorDatabaseService vectorDatabaseService)
    {
        _embeddingService = embeddingService;
        _vectorDatabaseService = vectorDatabaseService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 5, [FromQuery] Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query cannot be empty.");
        }

        // 1. Embed the query
        var embedding = await _embeddingService.GenerateEmbeddingAsync(query);

        // 2. Search Qdrant
        var filter = tenantId.HasValue
            ? new MetadataFilter { MustMatch = new Dictionary<string, object> { { "TenantId", tenantId.Value } } }
            : null;

        var results = await _vectorDatabaseService.SearchAsync<ExtractedTaskPayload>(
            "extracted_tasks",
            embedding,
            limit,
            filter);

        return Ok(results);
    }
}
