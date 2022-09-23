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

namespace Synapse.Integration.Models
{
    /// <summary>
    /// Describes an object used to reference workflows
    /// </summary>
    public class V1WorkflowReference
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowReference"/>
        /// </summary>
        public V1WorkflowReference()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowReference"/>
        /// </summary>
        /// <param name="id">The id of the workflow to reference</param>
        /// <param name="version">The version of the workflow to reference</param>
        public V1WorkflowReference(string id, string version = null) 
        {
            this.Id = id;
            this.Version = version;
        }

        /// <summary>
        /// Gets the id of the workflow to reference
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// Gets the version of the workflow to reference
        /// </summary>
        public virtual string Version { get; set; }

    }

}
