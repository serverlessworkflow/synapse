using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neuroglia.K8s;
using Synapse.Operator.Application.Services;
using Synapse.Domain.Models;
using Synapse.Services;

namespace Synapse.Operator.Application.Configuration
{

    public static class ConfigurationExtensions
    {

        public static IServiceCollection AddSynapseOperator(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.Configure<ApplicationOptions>(configuration);

            services.AddKubernetesClient();

            services.AddSingleton<IRepository<V1Workflow>, K8sRepository<V1Workflow>>();

            services.AddSingleton<IRepository<V1WorkflowInstance>, K8sRepository<V1WorkflowInstance>>();

            services.AddResourceWatcherFactory();

            services.AddResourceController<V1WorkflowController, V1Workflow>(builder => builder.ConfigureWatcher(watch => watch.InNamespace(SynapseConstants.EnvironmentVariables.Kubernetes.Namespace.Value)));

            services.AddResourceController<V1WorkflowInstanceController, V1WorkflowInstance>(builder => builder.ConfigureWatcher(watch => watch.InNamespace(SynapseConstants.EnvironmentVariables.Kubernetes.Namespace.Value)));

            return services;
        }

    }

}
