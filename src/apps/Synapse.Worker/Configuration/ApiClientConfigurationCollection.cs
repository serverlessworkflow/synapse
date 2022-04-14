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

namespace Synapse.Worker.Configuration
{

    /// <summary>
    /// Represents the object used to configure the Synapse APIs clients used to by the application
    /// </summary>
    public class ApiClientConfigurationCollection
    {

        /// <summary>
        /// Gets the default Synapse server's host name
        /// </summary>
        public static string DefaultHostName
        {
            get
            {
                var env = EnvironmentVariables.Api.HostName.Value;
                if (!string.IsNullOrWhiteSpace(env))
                    return env;
                return "synapse";
            }
        }

        /// <summary>
        /// Gets/sets the Synapse server's host name. Defaults to <see cref="DefaultHostName"/>
        /// </summary>
        public virtual string HostName { get; set; } = DefaultHostName;

        /// <summary>
        /// Gets/sets an object used to configure Synapse http-based APIs
        /// </summary>
        public virtual ApiClientConfiguration Http { get; set; } = ApiClientConfiguration.HttpDefault;

        /// <summary>
        /// Gets/sets an object used to configure Synapse GRPC-based APIs
        /// </summary>
        public virtual ApiClientConfiguration Grpc { get; set; } = ApiClientConfiguration.GrpcDefault;

    }

}
