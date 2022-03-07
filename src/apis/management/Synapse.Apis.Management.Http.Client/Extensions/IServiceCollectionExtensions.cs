/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Synapse.Apis.Management;
using Synapse.Apis.Management.Http;

namespace Synapse
{
    /// <summary>
    /// Defines extensions for <see cref="IServiceCollection"/>s
    /// </summary>
    public static class IServiceCollectionExtensions
    {

        /// <summary>
        /// Adds and configures a client for the Synapse HTTP REST API
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <param name="httpClientSetup">An <see cref="Action{T}"/> used to configure the <see cref="HttpClient"/> to use</param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSynapseRestApiClient(this IServiceCollection services, Action<HttpClient> httpClientSetup)
        {
            services.AddHttpClient(typeof(SynapseHttpManagementApiClient).Name, http => httpClientSetup(http));
            services.TryAddSingleton<ISynapseManagementApi, SynapseHttpManagementApiClient>();
            return services;
        }

    }

}
