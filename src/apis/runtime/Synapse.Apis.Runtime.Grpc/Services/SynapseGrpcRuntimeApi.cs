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

using ProtoBuf.Grpc;

namespace Synapse.Apis.Runtime.Grpc.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ISynapseGrpcRuntimeApi"/> interface
    /// </summary>
    public class SynapseGrpcRuntimeApi
        : ISynapseGrpcRuntimeApi
    {

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<SynapseRuntimeApiMessage> Connect(IAsyncEnumerable<SynapseRuntimeApiMessage> stream, CallContext context = default)
        {
            //todo: handshake
            //todo: register runtime (and expose request message stream)
            //todo: consume incoming messages
            return outboundStream.ReadAllAsync(context.CancellationToken);
        }

    }

}
