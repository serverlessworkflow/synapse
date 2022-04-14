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

namespace Synapse.Runtime.Docker
{

    /// <summary>
    /// Exposes constants about environment variables used by the Synapse Docker Workflow Runtime Host
    /// </summary>
    public static class EnvironmentVariables
    {

        /// <summary>
        /// Gets the prefix for all environment variables used by the Synapse Docker Workflow Runtime Host
        /// </summary>
        public const string Prefix = Synapse.EnvironmentVariables.Runtime.Prefix + "DOCKER_";

        /// <summary>
        /// Exposes constants about container-related environment variables
        /// </summary>
        public static class Container
        {

            /// <summary>
            /// Gets the prefix for all container-related environment variables
            /// </summary>
            public const string Prefix = EnvironmentVariables.Prefix + "CONTAINER_";

            /// <summary>
            /// Exposes constants about container configuration environment variable
            /// </summary>
            public static class Configuration
            {

                /// <summary>
                /// Gets the name of the container configuration environment variable
                /// </summary>
                public const string Name = Prefix + "SOURCE";

                /// <summary>
                /// Gets the value of the container configuration environment variable
                /// </summary>
                public static string? Value = Environment.GetEnvironmentVariable(Name);

            }

        }

    }
}
