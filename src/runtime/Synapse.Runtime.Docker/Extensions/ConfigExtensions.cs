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
    /// Defines extensions for <see cref="Config"/>s
    /// </summary>
    public static class ConfigExtensions
    {

        /// <summary>
        /// Adds the specified environment variable
        /// </summary>
        /// <param name="config">The extended <see cref="Config"/></param>
        /// <param name="name">The name of the environment variable to set</param>
        /// <param name="value">The new value of the environment variable to set</param>
        public static void AddOrUpdateEnvironmentVariable(this Config config, string name, string value)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (config.Env == null)
                config.Env = new List<string>();
            var variable = config.Env.FirstOrDefault(s => s.Split('=', StringSplitOptions.RemoveEmptyEntries)[0] == name);
            if (!string.IsNullOrEmpty(variable))
                config.Env.Remove(variable);
            variable = $"{name}={value}";
            config.Env.Add(variable);
        }

        /// <summary>
        /// Removes the environment variable with the specified name
        /// </summary>
        /// <param name="config">The extended <see cref="Config"/></param>
        /// <param name="name">The name of the environment variable to remove</param>
        /// <returns>A boolean indicating whether or not the specified environment variable has been removed</returns>
        public static bool RemoveEnvironmentVariable(this Config config, string name)
        {
            if (config.Env == null)
                return false;
            var variable = config.Env.FirstOrDefault(s => s.Split('=', StringSplitOptions.RemoveEmptyEntries)[0] == name);
            if (!string.IsNullOrEmpty(variable))
                return config.Env.Remove(variable);
            return false;
        }

    }

}
