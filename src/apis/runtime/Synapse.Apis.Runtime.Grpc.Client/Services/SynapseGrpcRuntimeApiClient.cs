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

using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Synapse.Apis.Runtime.Grpc
{

    /// <summary>
    /// Represents the default GRPC-based implementation of the <see cref="ISynapseRuntimeApi"/> interface
    /// </summary>
    public class SynapseGrpcRuntimeApiClient
        : ISynapseRuntimeApi
    {

        /// <summary>
        /// Initializes a new <see cref="ISynapseGrpcApi"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="runtimeApi">The service used to interact with the GRPC port of the Synapse Runtime API</param>
        public SynapseGrpcRuntimeApiClient(ILogger<SynapseGrpcRuntimeApiClient> logger, ISynapseGrpcRuntimeApi runtimeApi)
        {
            this.Logger = logger;
            this.RuntimeApi = runtimeApi;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to interact with the GRPC port of the Synapse Runtime API
        /// </summary>
        protected ISynapseGrpcRuntimeApi RuntimeApi { get; }

        /// <inheritdoc/>
        public virtual SynapseRuntimeApiSession Connect()
        {
            var requestStream = Channel.CreateUnbounded<SynapseRuntimeApiMessage>();
            var responseStream = this.RuntimeApi.Connect(requestStream.Reader.ReadAllAsync());
            return new(requestStream, responseStream);
        }

    }

}
