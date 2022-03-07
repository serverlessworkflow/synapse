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
using Neuroglia.Serialization;
using System.Runtime.Serialization;

namespace Synapse.Apis.Runtime
{

    /// <summary>
    /// Represents messages sent and consumed by the Synapse Runtime API
    /// </summary>
    [DataContract]
    public class SynapseRuntimeApiMessage
    {

        /// <summary>
        /// Initializes a new <see cref="SynapseRuntimeApiMessage"/>
        /// </summary>
        protected SynapseRuntimeApiMessage()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="SynapseRuntimeApiMessage"/>
        /// </summary>
        public SynapseRuntimeApiMessage(string subject, Dynamic? content = null)
        {
            if(string.IsNullOrWhiteSpace(subject))
                throw new ArgumentNullException(nameof(subject));
            this.Subject = subject;
            this.Content = content;
        }

        /// <summary>
        /// Gets the <see cref="SynapseRuntimeApiMessage"/>'s subject
        /// </summary>
        [DataMember(Order = 1)]
        public virtual string Subject { get; protected set; } = null!;

        /// <summary>
        /// Gets the message's content
        /// </summary>
        [DataMember(Order = 2)]
        public virtual Dynamic? Content { get; protected set; } = null!;

    }

}
