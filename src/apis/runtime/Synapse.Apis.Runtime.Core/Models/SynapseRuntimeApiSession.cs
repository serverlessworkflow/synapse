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
using System.Threading.Channels;

namespace Synapse.Apis.Runtime
{
    /// <summary>
    /// Represents a bi-directional Synapse Runtime API session
    /// </summary>
    public class SynapseRuntimeApiSession
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseRuntimeApiSession"/>
        /// </summary>
        /// <param name="requestStream">The session's request stream</param>
        /// <param name="responseStream">The session's response stream</param>
        public SynapseRuntimeApiSession(Channel<SynapseRuntimeApiMessage> requestStream, IAsyncEnumerable<SynapseRuntimeApiMessage> responseStream)
        {
            this.RequestStream = requestStream.Writer;
            this.ResponseStream = responseStream;
        }

        /// <summary>
        /// Gets the session's request stream
        /// </summary>
        public virtual ChannelWriter<SynapseRuntimeApiMessage> RequestStream { get; }

        /// <summary>
        /// Gets the session's response stream
        /// </summary>
        public virtual IAsyncEnumerable<SynapseRuntimeApiMessage> ResponseStream { get; }

    }

}
