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
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}
