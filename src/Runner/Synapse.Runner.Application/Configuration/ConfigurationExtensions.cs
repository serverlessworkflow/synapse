using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neuroglia.K8s;
using Synapse.Domain.Models;
using Synapse.Runner.Application.Services;
using Synapse.Services;

namespace Synapse.Runner.Application.Configuration
{

    /// <summary>
    /// Defines configuration extensions for <see cref="IServiceCollection"/>s
    /// </summary>
    public static class ConfigurationExtensions
    {

        /// <summary>
        /// Adds and configures the service required for the Synapse Runner application
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <param name="configuration">The current <see cref="IConfiguration"/></param>
        /// <param name="environment">The current <see cref="IHostEnvironment"/></param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSynapseRunner(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.Configure<ApplicationOptions>(configuration);
            services.AddCloudEventBus();
            services.AddKubernetesClient();
            services.AddSingleton<IExpressionEvaluatorFactory, ExpressionEvaluatorFactory>();

            services.AddSingleton<IRepository<V1Workflow>, K8sRepository<V1Workflow>>();
            services.AddSingleton<IRepository<V1WorkflowInstance>, K8sRepository<V1WorkflowInstance>>();
            services.AddSingleton<IRepository<V1Trigger>, K8sRepository<V1Trigger>>();

            services.AddSingleton<IExpressionEvaluatorFactory, ExpressionEvaluatorFactory>();

            services.AddSingleton<IWorkflowActivityProcessorFactory, WorkflowActivityProcessorFactory>();

            services.AddSingleton<IWorkflowExecutionContext, WorkflowExecutionContext>();

            services.AddSingleton<IWorkflowRunner, WorkflowRunner>();
            services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<IWorkflowRunner>());

            return services;
        }

    }

}
