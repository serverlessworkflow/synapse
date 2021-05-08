using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neuroglia.K8s;
using Synapse.Operator.Application.Services;
using Synapse.Domain.Models;
using Synapse.Services;

namespace Synapse.Operator.Application.Configuration
{

    /// <summary>
    /// Defines configuration extensions for <see cref="IServiceCollection"/>s
    /// </summary>
    public static class ConfigurationExtensions
    {

        /// <summary>
        /// Adds and configures the services required by the Synapse Operator runtime
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <param name="configuration">The current <see cref="IConfiguration"/></param>
        /// <param name="environment">The current <see cref="IHostEnvironment"/></param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSynapseOperator(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.Configure<ApplicationOptions>(configuration);

            services.AddKubernetesClient();

            services.AddSingleton<IRepository<V1Workflow>, K8sRepository<V1Workflow>>();
            services.AddSingleton<IRepository<V1WorkflowInstance>, K8sRepository<V1WorkflowInstance>>();
            services.AddSingleton<IRepository<V1Trigger>, K8sRepository<V1Trigger>>();

            services.AddResourceWatcherFactory();

            services.AddResourceController<V1WorkflowController, V1Workflow>(builder => builder.ConfigureWatcher(watch => watch.InNamespace(SynapseConstants.EnvironmentVariables.Kubernetes.Namespace.Value)));

            services.AddResourceController<V1WorkflowInstanceController, V1WorkflowInstance>(builder => builder.ConfigureWatcher(watch => watch.InNamespace(SynapseConstants.EnvironmentVariables.Kubernetes.Namespace.Value)));

            return services;
        }

    }

}
