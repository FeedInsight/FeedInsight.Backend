
using FeedInsight.API.Extensions;
using FeedInsight.Application;
using FeedInsight.Infrastructure;

namespace FeedInsight.API;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Register API services (which registers App and Infra internally)
        builder.Services.AddAPI(builder.Configuration);

        var app = builder.Build();

        // 2. Configure the HTTP request pipeline
        app.UseApiPipeline();

        // 3. Run the application
        app.Run();

        //Castle.Core
    }
}
