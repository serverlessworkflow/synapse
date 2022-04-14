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

using ProtoBuf;

namespace Synapse.Integration.Models
{

    /// <summary>
    /// Represents the base class for all the application's entity Data Transfer Objects
    /// </summary>
    [DataContract]
    [ProtoContract]
    [ProtoInclude(10, typeof(V1Workflow))]
    [ProtoInclude(20, typeof(V1WorkflowInstance))]
    [ProtoInclude(30, typeof(V1WorkflowActivity))]
    public abstract class Entity
        : DataTransferObject, IIdentifiable<string>
    {

        /// <inheritdoc/>
        [DataMember(Order = 1), ProtoMember(1)]
        public virtual string Id { get; set; }

        /// <inheritdoc/>
        [DataMember(Order = 2), ProtoMember(2)]
        public virtual DateTime CreatedAt { get; set; }

        /// <inheritdoc/>
        [DataMember(Order = 3), ProtoMember(3)]
        public virtual DateTime LastModified { get; set; }

        object IIdentifiable.Id => this.Id;

    }

}
