using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                ILogger logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation(SynapseOperatorConstants.Logging.GetHeader());
                await host.RunAsync();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            try
            {
                return Host.CreateDefaultBuilder(args)
                   .ConfigureServices((context, services) =>
                   {
                       services.AddSynapseOperator(context.Configuration, context.HostingEnvironment);
                   });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
           
        }

    }

}
