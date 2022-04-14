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
    /// Represents the options used to configure a client for a GRPC-based <see cref="ISynapseManagementApi"/>
    /// </summary>
    public class SynapseGrpcManagementApiClientOptions
    {

        /// <summary>
        /// Gets the default address of the GRPC-based <see cref="ISynapseManagementApi"/>
        /// </summary>
        public static Uri DefaultAddress
        {
            get
            {
                var scheme = EnvironmentVariables.Api.Grpc.Scheme.Value;
                if (string.IsNullOrWhiteSpace(scheme))
                    scheme = "http";
                var host = EnvironmentVariables.Api.HostName.Value;
                if (string.IsNullOrWhiteSpace(host))
                    scheme = "synapse";
                var port = EnvironmentVariables.Api.Grpc.Port.Value;
                if (string.IsNullOrWhiteSpace(port))
                    port = "41387";
                return new($"{scheme}://{host}:{port}");
            }
        }

        /// <summary>
        /// Gets/sets the address of the GRPC-based <see cref="ISynapseManagementApi"/> to connect to
        /// </summary>
        public virtual Uri Address { get; set; } = DefaultAddress;

        /// <summary>
        /// gets/sets the options used to configure the GRPC-based <see cref="ISynapseManagementApi"/>'s options
        /// </summary>
        public virtual GrpcChannelOptions ChannelOptions { get; set; } = new();

    }
}
