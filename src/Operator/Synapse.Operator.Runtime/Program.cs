using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Synapse.Operator.Application;
using Synapse.Operator.Application.Configuration;
using System;
using System.Threading.Tasks;

namespace Synapse.Operator.Runtime
{

    class Program
    {

        static async Task Main(string[] args)
        {
            using (IHost host = CreateHostBuilder(args).Build())
            {
                Microsoft.Extensions.Logging.ILogger logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation(SynapseOperatorConstants.Logging.GetHeader());
                await host.RunAsync();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder =>
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
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSynapseOperator(context.Configuration, context.HostingEnvironment);
                });
        }

    }

}
