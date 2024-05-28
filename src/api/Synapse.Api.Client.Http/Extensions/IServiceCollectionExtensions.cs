using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    /// <param name="httpClientSetup">An <see cref="Action{T}"/> used to configure the <see cref="HttpClient"/> to use</param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddSynapseHttpApiClient(this IServiceCollection services, Action<HttpClient> httpClientSetup)
    {
        services.AddHttpClient(typeof(SynapseHttpApiClient).Name, http => httpClientSetup(http));
        services.TryAddSingleton<ISynapseApiClient, SynapseHttpApiClient>();
        return services;
    }

}