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

using Synapse.Core.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Synapse.Core.Infrastructure.Containers;

/// <summary>
/// Defines extensions for <see cref="IServiceCollection"/>s
/// </summary>
public static class DockerContainerServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures a new <see cref="DockerContainerPlatform"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <param name="dockerClientConfiguration">The <see cref="DockerClientConfiguration"/> used to configure the <see cref="IDockerClient"/> to use, if any</param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddDockerContainerPlatform(this IServiceCollection services, DockerClientConfiguration? dockerClientConfiguration = null)
    {
        dockerClientConfiguration ??= new DockerClientConfiguration();
        services.TryAddSingleton<IDockerClient>(dockerClientConfiguration.CreateClient());
        services.TryAddSingleton<DockerContainerPlatform>();
        services.AddSingleton<IContainerPlatform>(provider => provider.GetRequiredService<DockerContainerPlatform>());
        services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<DockerContainerPlatform>());
        return services;
    }

}
