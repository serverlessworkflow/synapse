using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.Data.Infrastructure;
using Neuroglia.Data.Infrastructure.Mongo.Services;
using Neuroglia.Data.Infrastructure.ResourceOriented.Redis;
using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
using Neuroglia.Data.Infrastructure.Services;
using Neuroglia.Data.PatchModel.Services;
using Neuroglia.Mediation;
using Neuroglia.Plugins;
using Neuroglia.Security.Services;
using Neuroglia.Serialization;
using ServerlessWorkflow.Sdk.IO;
using Synapse.Resources;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="IServiceCollection"/>s
/// </summary>
public static class IServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures runtime infrastructure services
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <param name="configuration">The current <see cref="IConfiguration"/></param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddSynapse(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.AddSerialization();
        services.AddJsonSerializer();
        services.AddYamlDotNetSerializer();
        services.AddScoped<IUserInfoProvider, UserInfoProvider>();
        services.AddMediator();
        services.AddServerlessWorkflowIO();
        services.AddPluginProvider();

        var redisConnectionString = configuration.GetConnectionString(RedisDatabase.ConnectionStringName);
        services.AddPlugin(typeof(IDatabase), string.IsNullOrWhiteSpace(redisConnectionString) ? null : provider => provider.GetRequiredService<RedisDatabase>(), serviceLifetime: ServiceLifetime.Scoped);

        if (!string.IsNullOrWhiteSpace(redisConnectionString)) services.AddRedisDatabase(redisConnectionString, ServiceLifetime.Scoped);
        services.AddHostedService<Core.Infrastructure.Services.DatabaseInitializer>();

        services.AddPlugin(typeof(IRepository<Document>), provider => provider.GetRequiredService<MongoRepository<Document, string>>(), serviceLifetime: ServiceLifetime.Scoped);
        services.AddMongoDatabase("synapse");
        services.AddMongoRepository<Document, string>(lifetime: ServiceLifetime.Scoped);

        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IAdmissionControl, AdmissionControl>();
        services.AddScoped<IVersionControl, VersionControl>();
        services.AddSingleton<IPatchHandler, JsonMergePatchHandler>();
        services.AddSingleton<IPatchHandler, JsonPatchHandler>();
        services.AddSingleton<IPatchHandler, JsonStrategicMergePatchHandler>();

        return services;
    }

}