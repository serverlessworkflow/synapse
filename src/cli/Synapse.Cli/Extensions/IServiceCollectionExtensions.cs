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

using Neuroglia;

namespace Synapse.Cli;

/// <summary>
/// Defines extensions for <see cref="IServiceCollection"/>s
/// </summary>
public static class IServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures all <see cref="Command"/>s loaded from the specified assemblies
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddCliCommands(this IServiceCollection services)
    {
        foreach (var commandType in TypeCacheUtil.FindFilteredTypes
        (
            "synctl-cmd",
            t => t.IsClass && !t.IsAbstract && !t.IsInterface && !t.IsGenericType && typeof(Command).IsAssignableFrom(t) && t.IsPublic,
            typeof(Commands.Command).Assembly)
        )
        {
            services.AddScoped(typeof(Command), commandType);
        }
        return services;
    }

}

