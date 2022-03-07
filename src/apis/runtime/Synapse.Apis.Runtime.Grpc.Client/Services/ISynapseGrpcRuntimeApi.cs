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
using System.ServiceModel;

namespace Synapse.Apis.Runtime.Grpc
{

    /// <summary>
    /// Defines the fundamentals of the GRPC-based <see cref="ISynapseRuntimeApi"/>
    /// </summary>
    [ServiceContract]
    public interface ISynapseGrpcRuntimeApi
    {

        /// <summary>
        /// Connects to the Synapse Runtime API
        /// </summary>
        /// <param name="stream">A new <see cref="IAsyncEnumerable{T}"/> used to send messages to the API server</param>
        /// <param name="context">The current <see cref="CallContext"/></param>
        /// <returns>A new <see cref="IAsyncEnumerable{T}"/> used to consume messages sent by the API server</returns>
        [OperationContract]
        IAsyncEnumerable<SynapseRuntimeApiMessage> Connect(IAsyncEnumerable<SynapseRuntimeApiMessage> stream, CallContext context = default);

    }

}
