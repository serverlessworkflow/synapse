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

namespace Synapse.Apis.Runtime
{

    /// <summary>
    /// Represents a runtime signal
    /// </summary>
    [DataContract]
    public class V1RuntimeSignal
    {

        /// <summary>
        /// Initializes a new <see cref="V1RuntimeSignal"/>
        /// </summary>
        protected V1RuntimeSignal()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1RuntimeSignal"/>
        /// </summary>
        /// <param name="type">The <see cref="V1RuntimeSignal"/> type</param>
        /// <param name="data">The <see cref="V1RuntimeSignal"/>'s data</param>
        public V1RuntimeSignal(V1RuntimeSignalType type, Dynamic? data = null)
        {
            this.Type = type;
            this.Data = data;
        }

        /// <summary>
        /// Gets the <see cref="V1RuntimeSignal"/> type
        /// </summary>
        [DataMember(Order = 1)]
        public V1RuntimeSignalType Type { get; protected set; }

        /// <summary>
        /// Gets the <see cref="V1RuntimeSignal"/>'s data
        /// </summary>
        [DataMember(Order = 2)]
        public virtual Dynamic? Data { get; protected set; }

    }

}
