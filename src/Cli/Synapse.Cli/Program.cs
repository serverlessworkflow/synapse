using k8s;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.K8s;
using Newtonsoft.Json.Serialization;
using ServerlessWorkflow.Sdk;
using Synapse.Cli.Commands;
using Synapse.Cli.Services;
using Synapse.Domain.Models;
using Synapse.Services;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace Synapse.Cli
{

    class Program
    {

        async static Task<int> Main(string[] args)
        {
            Parser parser = BuildCommandLineParser();
            return await parser.InvokeAsync(args);
        }

        static Parser BuildCommandLineParser()
        {
            CommandLineBuilder builder = new CommandLineBuilder();
            IServiceProvider serviceProvider = BuildServiceProvider();
            var settings = serviceProvider.GetRequiredService<IKubernetes>().SerializationSettings;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            foreach (System.CommandLine.Command command in serviceProvider.GetServices<System.CommandLine.Command>())
            {
                builder.AddCommand(command);
            }
            return builder.UseDefaults().Build();
        }

        static IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClient();
            services.AddCliCommands(typeof(RunCommand));
            services.AddServerlessWorkflow();
            services.AddSingleton<ISynapseService, SynapseService>();
            services.AddKubernetesClient(KubernetesClientConfiguration.BuildConfigFromConfigFile());
            services.AddSingleton<IRepository<V1Workflow>, K8sRepository<V1Workflow>>();
            services.AddSingleton<IRepository<V1WorkflowInstance>, K8sRepository<V1WorkflowInstance>>();
            return services.BuildServiceProvider();
        }

    }

}
