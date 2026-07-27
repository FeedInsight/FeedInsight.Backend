using FeedInsight.Application.Common.Interfaces;

namespace FeedInsight.API.Middleware;

public class TenantStatusMiddleware
{
    private readonly RequestDelegate _next;

    public TenantStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantStatusChecker tenantStatusChecker)
    {
        // Skip tenant validation for unauthenticated requests
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // SuperAdmin has no TenantId, so skip tenant validation
        var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;

        if (string.IsNullOrWhiteSpace(tenantIdClaim))
        {
            await _next(context);
            return;
        }

        // Invalid TenantId claim
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            await _next(context);
            return;
        }

        // Check if the Tenant is active
        var result = await tenantStatusChecker.EnsureActiveAsync(
            tenantId,
            context.RequestAborted);

        if (result.IsError)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            await context.Response.WriteAsJsonAsync(new
            {
                Title = "Access denied.",
                Status = StatusCodes.Status403Forbidden,
                ErrorCode = result.FirstError.Code
            });

            return;
        }

        // Tenant is active, continue the request pipeline
        await _next(context);
    }
}