using Blogifier.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace GhostToBlogifier
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.RollingFile("Logs/{Date}.txt", LogEventLevel.Warning)
                .CreateLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBlogDatabase(Configuration);
            services.AddBlogServices();
        }
        
    }
}