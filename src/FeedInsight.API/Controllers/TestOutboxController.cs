using FeedInsight.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedInsight.API.Controllers;

[Route("api/test/outbox")]
[ApiController]
public class TestOutboxController : ControllerBase
{
    private readonly FeedInsightDbContext _dbContext;

    public TestOutboxController(FeedInsightDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("reset-failed")]
    public async Task<IActionResult> ResetFailedMessages(CancellationToken cancellationToken)
    {
        // Find messages that have been processed but have an error (permanently failed)
        var failedMessages = await _dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc != null && m.Error != null)
            .ToListAsync(cancellationToken);

        var count = failedMessages.Count;

        if (count > 0)
        {
            foreach (var message in failedMessages)
            {
                message.ProcessedOnUtc = null;
                message.RetryCount = 0;
                message.NextRetryUtc = null;
                // We don't clear the Error here so you can still see what the last error was,
                // but setting ProcessedOnUtc to null will allow the background processor to pick it up again.
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok(new
        {
            Message = $"Successfully reset {count} permanently failed outbox messages.",
            ResetCount = count
        });
    }
}
