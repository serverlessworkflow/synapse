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

using k8s;

namespace Synapse.Runtime.Docker.Configuration
{

    /// <summary>
    /// Represents the options used to configure a Synapse Docker-based runtime
    /// </summary>
    public class DockerRuntimeOptions
    {

        /// <summary>
        /// Gets the default Docker Runtime container image
        /// </summary>
        public const string DefaultContainerImage = "synapse/runtime-executor";

        private static readonly Config _DefaultContainerConfiguration = new()
        {
            Image = DefaultContainerImage
        };

        /// <summary>
        /// Gets the default Docker Runtime container configuration
        /// </summary>
        public static Config DefaultContainerConfiguration => _DefaultContainerConfiguration;

        /// <summary>
        /// Gets/sets the name of the image repository to use when pulling the runtime's container image
        /// </summary>
        public virtual string? ImageRepository { get; set; }

        /// <summary>
        /// Gets/sets the Docker image pull policy
        /// </summary>
        public virtual ImagePullPolicy ImagePullPolicy { get; set; }

        /// <summary>
        /// Gets/sets the configuration of the runtime container
        /// </summary>
        public virtual Config Container { get; set; } = LoadContainerConfiguration();

        /// <summary>
        /// Loads the Docker runtime container configuration
        /// </summary>
        /// <returns>The Docker runtime container configuration</returns>
        public static Config LoadContainerConfiguration()
        {
            var configFilePath = EnvironmentVariables.Container.Configuration.Value;
            if (string.IsNullOrWhiteSpace(configFilePath)
                || !File.Exists(configFilePath))
                return DefaultContainerConfiguration;
            var yaml = File.ReadAllText(configFilePath);
            return Yaml.LoadFromString<Config>(yaml);
        }

    }

}
