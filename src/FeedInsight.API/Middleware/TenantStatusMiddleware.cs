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
        HttpContext context,ITenantResolver tenantResolver,ITenantStatusChecker tenantStatusChecker)
    {
        var tenantId = await tenantResolver.ResolveTenantIdAsync( context.RequestAborted);

       
        if (tenantId is null)
        {
            await _next(context);
            return;
        }

        var result = await tenantStatusChecker.EnsureActiveAsync(
            tenantId.Value,
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

        await _next(context);
    }
}