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

namespace Synapse.Dashboard.Services
{

    /// <summary>
    /// Represents the default, serialization-based <see cref="ICloner"/> implementation
    /// </summary>
    public class Cloner
        : ICloner
    {

        /// <summary>
        /// Initializes a new <see cref="Cloner"/>
        /// </summary>
        /// <param name="serializer">The <see cref="IJsonSerializer"/> used to clone objects</param>
        public Cloner(IJsonSerializer serializer)
        {
            this.Serializer = serializer;
        }

        /// <summary>
        /// Gets the <see cref="IJsonSerializer"/> used to clone objects
        /// </summary>
        protected IJsonSerializer Serializer { get; set; }

        /// <inheritdoc/>
        public T Clone<T>(T obj) => this.Serializer.Deserialize<T>(this.Serializer.Serialize(obj))!;

        /// <inheritdoc/>
        public async Task<T> CloneAsync<T>(T obj, CancellationToken cancellationToken) => await this.Serializer.DeserializeAsync<T>(await this.Serializer.SerializeAsync(obj))!;

    }
}
