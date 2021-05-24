using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Synapse.Configuration;
using Synapse.Services;
using System;
using System.Reactive.Subjects;

namespace Synapse
{

    /// <summary>
    /// Defines configuration-related extensions for Synapse infrastructure services
    /// </summary>
    public static class ConfigurationExtensions
    {

        /// <summary>
        /// Adds and configures a <see cref="CloudEventBus"/>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <param name="configurationAction">The <see cref="Action{T}"/> used to configure the <see cref="CloudEventBusOptions"/></param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddCloudEventBus(this IServiceCollection services, Action<CloudEventBusOptions> configurationAction)
        {
            services.Configure(configurationAction);
            services.AddSingleton<ICloudEventFormatter, JsonEventFormatter>();
            services.AddSingleton<Subject<CloudEvent>>();
            services.AddHttpClient(nameof(CloudEventBus), http => { });
            services.AddSingleton<ICloudEventBus, CloudEventBus>();
            services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<ICloudEventBus>());
            return services;
        }

        /// <summary>
        /// Adds and configures a <see cref="CloudEventBus"/>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddCloudEventBus(this IServiceCollection services)
        {
            return services.AddCloudEventBus(options => { });
        }

    }

}
