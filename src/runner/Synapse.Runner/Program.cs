using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Synapse.Api.Client;
using Synapse.Runner.Services;
using System.Diagnostics;

if (args.Length != 0 && args.Contains("--debug") && !Debugger.IsAttached) Debugger.Launch();

var builder = Host.CreateDefaultBuilder()
     .ConfigureAppConfiguration(config =>
     {
         config.AddJsonFile("appsettings.json", true, true);
         config.AddEnvironmentVariables("SYNAPSE");
         config.AddCommandLine(args);
         config.AddKeyPerFile("/run/secrets/synapse", true, true);
     })
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        services.AddSynapseHttpApiClient(http => { });
        services.AddHostedService<WorkflowExecutorInitializer>();
    });

using var app = builder.Build();

await app.RunAsync();