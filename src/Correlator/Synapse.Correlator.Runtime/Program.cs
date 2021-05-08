using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Synapse.Correlator.Runtime
{

    public class Program
    {

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging((ILoggingBuilder builder) =>
                    {
                        builder.ClearProviders();
                        Log.Logger = new LoggerConfiguration()
                            .MinimumLevel
                                .Override("Microsoft", LogEventLevel.Information)
                            .Enrich
                                .FromLogContext()
                            .WriteTo
                                .Console()
                            .CreateLogger();
                        builder.AddSerilog(dispose: true);
                    });
                    webBuilder.UseStartup<Startup>();
                });
        }

    }

}
