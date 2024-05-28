using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Synapse.Api.Client.Http.Configuration;
using Synapse.Api.Client.Services;

namespace Synapse.Api.Client;

/// <summary>
/// Defines extensions for <see cref="IServiceCollection"/>s
/// </summary>
public static class IServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures a client for the Synapse HTTP API
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <param name="setup">An <see cref="Action{T}"/> used to configure the <see cref="SynapseHttpApiClientOptions"/> to use</param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddSynapseHttpApiClient(this IServiceCollection services, Action<SynapseHttpApiClientOptions> setup)
    {
        services.Configure(setup);
        services.AddHttpClient(typeof(SynapseHttpApiClient).Name, (provider, http) =>
        {
            http.BaseAddress = provider.GetRequiredService<IOptions<SynapseHttpApiClientOptions>>().Value.BaseAddress;
        });
        services.TryAddScoped<ISynapseApiClient, SynapseHttpApiClient>();
        services.TryAddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<SynapseHttpApiClientOptions>>().Value;
            var connection = new HubConnectionBuilder()
                .WithUrl(new Uri(options.BaseAddress, "api/ws/resources/watch"))
                .WithAutomaticReconnect()
                .Build();
            return new ResourceWatchEventHubClient(connection);
        });
        return services;
    }

}