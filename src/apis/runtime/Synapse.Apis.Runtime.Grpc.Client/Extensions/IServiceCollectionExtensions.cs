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

using Synapse.Apis.Runtime.Grpc.Configuration;

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
        /// <param name="configurationAction">An <see cref="Action{T}"/> used to configure the <see cref="SynapseGrpcRuntimeApiClientOptions"/> to use</param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSynapseGrpcRuntimeApiClient(this IServiceCollection services, Action<ISynapseGrpcRuntimeApiClientOptionsBuilder> configurationAction)
        {
            if (configurationAction == null)
                throw new ArgumentNullException(nameof(configurationAction));
            var builder = new SynapseGrpcRuntimeApiClientOptionsBuilder();
            configurationAction(builder);
            var options = builder.Build();
            services.TryAddSingleton(Options.Create(options));
            services.TryAddSingleton(provider =>
            {
                var channel = GrpcChannel.ForAddress(options.Address, options.ChannelOptions);
                return channel.CreateGrpcService<ISynapseGrpcRuntimeApi>();
            });
            services.TryAddSingleton<ISynapseRuntimeApi, SynapseGrpcRuntimeApiClient>();
            return services;
        }

        /// <summary>
        /// Adds and configures a GRPC-based client for the Synapse Runtime API
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSynapseGrpcRuntimeApiClient(this IServiceCollection services)
        {
            return services.AddSynapseGrpcRuntimeApiClient(builder => { });
        }

    }
}
