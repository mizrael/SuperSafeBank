using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Loki;

namespace SuperSafeBank.Web.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, builder)=>
                {
                    builder.AddJsonFile("appsettings.json", optional: false);
                    
#if OnPremise
                    builder.AddJsonFile($"appsettings.OnPremise-{ctx.HostingEnvironment.EnvironmentName}.json", optional: true);
#endif

#if OnAzure
                    builder.AddJsonFile($"appsettings.OnAzure-{ctx.HostingEnvironment.EnvironmentName}.json", optional: true);
#endif

                    builder.AddUserSecrets<Startup>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).UseSerilog((ctx, cfg) =>
                {
                    var credentials = new NoAuthCredentials(ctx.Configuration.GetConnectionString("loki"));

                    cfg.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Application", ctx.HostingEnvironment.ApplicationName)
                        .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
                        .WriteTo.Console(new RenderedCompactJsonFormatter())
                        .WriteTo.LokiHttp(credentials);
                });
    }
}
