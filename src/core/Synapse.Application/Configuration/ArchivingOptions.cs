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

namespace Synapse.Application.Configuration
{
    /// <summary>
    /// Represents the options used to configure the way Synapse archives <see cref="V1WorkflowInstance"/>s
    /// </summary>
    public class ArchivingOptions
    {

        /// <summary>
        /// Gets/sets the type of <see cref="ISerializer"/> to use to archive <see cref="V1WorkflowInstance"/>s
        /// </summary>
        public virtual Type SerializerType { get; set; } = typeof(NewtonsoftJsonSerializer);

        /// <summary>
        /// Gets/sets the <see cref="V1WorkflowInstance"/> file extension
        /// </summary>
        public virtual string FileExtension { get; set; } = ".json";

    }

}
