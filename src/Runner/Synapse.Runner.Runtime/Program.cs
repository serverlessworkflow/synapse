using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Synapse.Runner.Runtime
{

    /// <summary>
    /// Exposes the application's entry point
    /// </summary>
    public class Program
    {

        /// <summary>
        /// The application's entry point
        /// </summary>
        /// <param name="args">The application's arguments</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates a new <see cref="IHostBuilder"/>
        /// </summary>
        /// <param name="args">The application's arguments</param>
        /// <returns>A new <see cref="IHostBuilder"/></returns>
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
