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
using Synapse.Apis.Runtime.Grpc.Configuration;

namespace Synapse.Apis.Runtime.Grpc
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ISynapseGrpcRuntimeApiClientOptionsBuilder"/> interface
    /// </summary>
    public class SynapseGrpcRuntimeApiClientOptionsBuilder
        : ISynapseGrpcRuntimeApiClientOptionsBuilder
    {

        /// <summary>
        /// Gets the <see cref="SynapseGrpcRuntimeApiClientOptions"/> to configure
        /// </summary>
        protected SynapseGrpcRuntimeApiClientOptions Options { get; } = new();

        /// <inheritdoc/>
        public virtual ISynapseGrpcRuntimeApiClientOptionsBuilder ForAddress(Uri address)
        {
            if(address == null)
                throw new ArgumentNullException(nameof(address));
            this.Options.Address = address;
            return this;
        }

        /// <inheritdoc/>
        public virtual ISynapseGrpcRuntimeApiClientOptionsBuilder WithChannelOptions(GrpcChannelOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            this.Options.ChannelOptions = options;
            return this;
        }

        /// <inheritdoc/>
        public virtual ISynapseGrpcRuntimeApiClientOptionsBuilder WithChannelOptions(Action<GrpcChannelOptions> configurationAction)
        {
            if (configurationAction == null)
                throw new ArgumentNullException(nameof(configurationAction));
            var options = new GrpcChannelOptions();
            configurationAction(options);
            return this.WithChannelOptions(options);
        }

        /// <inheritdoc/>
        public virtual SynapseGrpcRuntimeApiClientOptions Build()
        {
            return this.Options;
        }

    }
}
