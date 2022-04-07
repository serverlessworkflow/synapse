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

namespace Synapse.Apis.Management.Grpc.Configuration
{

    /// <summary>
    /// Defines the fundamentals of a service used to build <see cref="SynapseGrpcManagementApiClientOptions"/>
    /// </summary>
    public interface ISynapseGrpcManagementApiClientOptionsBuilder
    {

        /// <summary>
        /// Configures the address of the GRPC-based API to connect to
        /// </summary>
        /// <param name="address">The  address of the GRPC-based API to connect to</param>
        /// <returns>The configured <see cref="ISynapseGrpcManagementApiClientOptionsBuilder"/></returns>
        ISynapseGrpcManagementApiClientOptionsBuilder ForAddress(Uri address);

        /// <summary>
        /// Configures the <see cref="ISynapseGrpcManagementApiClientOptionsBuilder"/> to use the specified <see cref="GrpcChannelOptions"/>
        /// </summary>
        /// <param name="options">The <see cref="GrpcChannelOptions"/> to use</param>
        /// <returns>The configured <see cref="ISynapseGrpcManagementApiClientOptionsBuilder"/></returns>
        ISynapseGrpcManagementApiClientOptionsBuilder WithChannelOptions(GrpcChannelOptions options);

        /// <summary>
        /// Configures the <see cref="ISynapseGrpcManagementApiClientOptionsBuilder"/> to use the specified <see cref="GrpcChannelOptions"/>
        /// </summary>
        /// <param name="configurationAction">The <see cref="Action{T}"/> used to configure the <see cref="GrpcChannelOptions"/> to use</param>
        /// <returns>The configured <see cref="ISynapseGrpcManagementApiClientOptionsBuilder"/></returns>
        ISynapseGrpcManagementApiClientOptionsBuilder WithChannelOptions(Action<GrpcChannelOptions> configurationAction);

        /// <summary>
        /// Builds the <see cref="SynapseGrpcManagementApiClientOptions"/> to use
        /// </summary>
        /// <returns>New <see cref="SynapseGrpcManagementApiClientOptions"/></returns>
        SynapseGrpcManagementApiClientOptions Build();

    }


}
