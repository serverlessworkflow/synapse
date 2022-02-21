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

using Synapse.Runtime.Services;

namespace Synapse.Runtime.Docker.Configuration
{

    /// <summary>
    /// Represents the options used to configure a <see cref="DockerRuntimeHost"/>
    /// </summary>
    public class DockerRuntimeHostOptions
    {

        /// <summary>
        /// Gets/sets the Docker runtime host's network name
        /// </summary>
        public virtual string Network { get; set; } = "synapse";

        /// <summary>
        /// Gets the object used to configure the workflow runtime container configuration
        /// </summary>
        public virtual DockerRuntimeOptions Runtime { get; set; } = new();

    }

}
