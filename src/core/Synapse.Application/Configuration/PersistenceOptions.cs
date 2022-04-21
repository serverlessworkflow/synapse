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

namespace Synapse.Application.Configuration
{
    /// <summary>
    /// Represents the options used to configure the application's persistence layer
    /// </summary>
    public class PersistenceOptions
    {

        /// <summary>
        /// Initializes a new <see cref="PersistenceOptions"/>
        /// </summary>
        public PersistenceOptions()
        {
            var env = EnvironmentVariables.Persistence.WriteModel.DefaultRepository.Value;
            if (!string.IsNullOrWhiteSpace(env))
                this.DefaultWriteModelRepository = new() { PluginName = env };
            env = EnvironmentVariables.Persistence.ReadModel.DefaultRepository.Value;
            if (!string.IsNullOrWhiteSpace(env))
                this.DefaultReadModelRepository = new() { PluginName = env };
        }

        /// <summary>
        /// Gets/sets the options used to configure the default write model <see cref="IRepository"/>
        /// </summary>
        public virtual RepositoryOptions DefaultWriteModelRepository { get; set; } = new();

        /// <summary>
        /// Gets/sets the options used to configure the default read model <see cref="IRepository"/>
        /// </summary>
        public virtual RepositoryOptions DefaultReadModelRepository { get; set; } = new();

        /// <summary>
        /// Gets/sets a <see cref="Dictionary{TKey, TValue}"/> containing the mappings of of entity type full names to the <see cref="RepositoryOptions"/> to use
        /// </summary>
        public virtual Dictionary<string, RepositoryOptions> Repositories { get; set; } = new();

    }

}
