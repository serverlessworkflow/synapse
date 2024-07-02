// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.Data.Infrastructure;
using Neuroglia.Data.Infrastructure.Redis.Services;
using Neuroglia.Data.Infrastructure.ResourceOriented.Redis;
using Neuroglia.Data.Infrastructure.ResourceOriented.Services;
using Neuroglia.Data.Infrastructure.Services;
using Neuroglia.Data.PatchModel.Services;
using Neuroglia.Mediation;
using Neuroglia.Plugins;
using Neuroglia.Security.Services;
using Neuroglia.Serialization;
using ServerlessWorkflow.Sdk.IO;
using Synapse.Core.Infrastructure.Services;
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
        services.AddMediator();
        services.AddScoped<IUserInfoProvider, UserInfoProvider>();
        services.AddServerlessWorkflowIO();
        services.AddPluginProvider();

        var redisConnectionString = configuration.GetConnectionString(RedisDatabase.ConnectionStringName);
        services.AddPlugin(typeof(IDatabase), string.IsNullOrWhiteSpace(redisConnectionString) ? null : provider => provider.GetRequiredService<RedisDatabase>(), serviceLifetime: ServiceLifetime.Singleton);

        if (!string.IsNullOrWhiteSpace(redisConnectionString)) services.AddRedisDatabase(redisConnectionString, ServiceLifetime.Singleton);
        services.AddHostedService<Core.Infrastructure.Services.DatabaseInitializer>();

        services.AddPlugin(typeof(IRepository<Document>), provider => provider.GetRequiredService<RedisRepository<Document, string>>(), serviceLifetime: ServiceLifetime.Scoped);
        services.AddRedisRepository<Document, string>(lifetime: ServiceLifetime.Scoped);

        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IAdmissionControl, AdmissionControl>();
        services.AddScoped<IVersionControl, VersionControl>();
        services.AddScoped<IResourceMutator, WorkflowInstanceMutator>();
        services.AddSingleton<IPatchHandler, JsonMergePatchHandler>();
        services.AddSingleton<IPatchHandler, JsonPatchHandler>();
        services.AddSingleton<IPatchHandler, JsonStrategicMergePatchHandler>();

        return services;
    }

}
