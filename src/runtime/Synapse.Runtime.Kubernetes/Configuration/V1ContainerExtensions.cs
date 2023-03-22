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

namespace Synapse.Runtime
{
    /// <summary>
    /// Defines extensions for <see cref="V1Container"/>s
    /// </summary>
    public static class V1ContainerExtensions
    {

        /// <summary>
        /// Adds the specified environment variable
        /// </summary>
        /// <param name="config">The extended <see cref="V1Container"/></param>
        /// <param name="variable">The environment variable to set</param>
        public static void AddOrUpdateEnvironmentVariable(this V1Container config, V1EnvVar variable)
        {
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));
            if (config.Env == null)
                config.Env = new List<V1EnvVar>();
            var existingVariable = config.Env.FirstOrDefault(e => e.Name.Equals(variable.Name, StringComparison.OrdinalIgnoreCase));
            if (existingVariable != null)
                config.Env.Remove(existingVariable);
            config.Env.Add(variable);
        }

        /// <summary>
        /// Removes the environment variable with the specified name
        /// </summary>
        /// <param name="config">The extended <see cref="V1Container"/></param>
        /// <param name="variable">The  environment variable to remove</param>
        /// <returns>A boolean indicating whether or not the specified environment variable has been removed</returns>
        public static bool RemoveEnvironmentVariable(this V1Container config, V1EnvVar variable)
        {
            if (config.Env == null)
                return false;
            var existingVariable = config.Env.FirstOrDefault(e => e.Name.Equals(variable.Name, StringComparison.OrdinalIgnoreCase));
            if (existingVariable != null)
                return config.Env.Remove(existingVariable);
            return false;
        }

    }

}
