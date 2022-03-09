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

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents a <see cref="CloudEvent"/>
    /// </summary>
    [DataTransferObjectType(typeof(Integration.Models.V1CloudEvent))]
    public class V1CloudEvent
    {

        /// <summary>
        /// Initializes a new <see cref="V1CloudEvent"/>
        /// </summary>
        protected V1CloudEvent()
        {
            this.Id = null!;
            this.Source = null!;
            this.Type = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="V1CloudEvent"/>
        /// </summary>
        /// <param name="id">The cloud event's id</param>
        /// <param name="source">The cloud event's source <see cref="Uri"/></param>
        /// <param name="type">cloud event's type</param>
        /// <param name="specVersion">The cloud event's spec version</param>
        public V1CloudEvent(string id, Uri source, string type, CloudEventsSpecVersion? specVersion = null)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException(nameof(type));
            if (specVersion == null)
                specVersion = CloudEventsSpecVersion.V1_0;
            this.Id = id;
            this.Source = source;
            this.SpecVersion = specVersion.VersionId;
            this.Type = type;
        }

        /// <summary>
        /// Gets the cloud event's <see href="https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md#id">id</see>
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the cloud event's <see href="https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md#source">source</see> <see cref="Uri"/>
        /// </summary>
        public virtual Uri Source { get; protected set; }

        /// <summary>
        /// Gets the cloud event's <see href="https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md#specversion">spec version</see>
        /// </summary>
        public virtual string SpecVersion { get; protected set; } = CloudEventsSpecVersion.V1_0.VersionId;

        /// <summary>
        /// Gets the cloud event's <see href="https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md#type">type</see>
        /// </summary>
        public virtual string Type { get; protected set; }

        /// <summary>
        /// Gets the cloud event's data content type
        /// </summary>
        public virtual string? DataContentType { get; protected set; }

        /// <summary>
        /// Gets the cloud event's data schema <see cref="Uri"/>, if any
        /// </summary>
        public virtual Uri? DataSchema { get; protected set; }

        /// <summary>
        /// Gets the cloud event's subject
        /// </summary>
        public virtual string? Subject { get; protected set; }

        /// <summary>
        /// Gets the cloud event's type
        /// </summary>
        public virtual DateTime Time { get; protected set; }

        /// <summary>
        /// Gets the cloud event's data
        /// </summary>
        public virtual object? Data { get; protected set; }

        /// <summary>
        /// Gets an <see cref="IDictionary{TKey, TValue}"/> that contains the cloud event's extensions
        /// </summary>
        public virtual IDictionary<string, object> Extensions { get; protected set; } = new Dictionary<string, object>();

    }

}
