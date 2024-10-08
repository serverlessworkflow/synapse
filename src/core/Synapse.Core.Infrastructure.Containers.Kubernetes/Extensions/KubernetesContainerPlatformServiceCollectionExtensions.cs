﻿// Copyright © 2024-Present The Synapse Authors
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
public static class KubernetesContainerPlatformServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures a new <see cref="KubernetesContainerPlatform"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddKubernetesContainerPlatform(this IServiceCollection services)
    {
        services.TryAddSingleton<KubernetesContainerPlatform>();
        services.AddSingleton<IContainerPlatform>(provider => provider.GetRequiredService<KubernetesContainerPlatform>());
        services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<KubernetesContainerPlatform>());
        return services;
    }

}
