namespace FeedInsight.API.Extensions
{
    public static class staticWebApplicationExtensions
    {
        public static WebApplication UseApiPipeline(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}
