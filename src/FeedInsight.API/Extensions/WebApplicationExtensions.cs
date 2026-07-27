using FeedInsight.API.Middleware;

namespace FeedInsight.API.Extensions
{
    public static class staticWebApplicationExtensions
    {
        public static WebApplication UseApiPipeline(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseCors(CorsExtensions.PolicyName);

            app.UseAuthentication();

            app.UseMiddleware<TenantStatusMiddleware>();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}
