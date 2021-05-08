using CloudNative.CloudEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neuroglia.K8s;
using Synapse.Correlator.Application.Services;
using Synapse.Domain.Models;
using Synapse.Services;
using System.Reactive.Subjects;

namespace Synapse.Runner.Application.Configuration
{

    /// <summary>
    /// Defines configuration extensions for the <see cref="IServiceCollection"/>
    /// </summary>
    public static class ConfigurationExtensions
    {

        /// <summary>
        /// Adds and configures the services required by the Synapse Broker application
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <param name="configuration">The current <see cref="IConfiguration"/></param>
        /// <param name="environment">The current <see cref="IHostEnvironment"/></param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSynapseBroker(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.AddHttpClient();

            services.AddSingleton<ICloudEventFormatter, JsonEventFormatter>();
            services.AddSingleton<Subject<CloudEvent>>();
            services.AddHttpClient(nameof(CloudEventBus), http => { });
            services.AddSingleton<ICloudEventBus, CloudEventBus>();

            services.AddKubernetesClient();

            services.AddSingleton<IRepository<V1WorkflowInstance>, K8sRepository<V1WorkflowInstance>>();
            services.AddSingleton<IRepository<V1Trigger>, K8sRepository<V1Trigger>>();

            services.AddSingleton<ICorrelationService, CorrelationService>();
            services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<ICorrelationService>());

            services.AddResourceWatcherFactory();

            return services;
        }

    }

}
