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
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProtoBuf.Grpc.Client;

namespace Synapse.Apis.Runtime.Grpc
{

    /// <summary>
    /// Defines extensions for <see cref="IServiceCollection"/>s
    /// </summary>
    public static class IServiceCollectionExtensions
    {

        /// <summary>
        /// Adds and configures a GRPC-based client for the Synapse Runtime API
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSynapseGrpcRuntimeApiClient(this IServiceCollection services)
        {
            services.TryAddSingleton(provider =>
            {
                var channel = GrpcChannel.ForAddress($"{EnvironmentVariables.Api.Host.Value}:8080"); //todo: config-based
                return channel.CreateGrpcService<ISynapseGrpcRuntimeApi>();
            });
            services.TryAddSingleton<ISynapseRuntimeApi, SynapseGrpcRuntimeApiClient>();
            return services;
        }

    }

}
