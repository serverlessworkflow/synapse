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

using Neuroglia.Data;

namespace Synapse.Infrastructure.Plugins
{

    /// <summary>
    /// Defines the fundamentals of an <see cref="IPlugin"/> used to create <see cref="IRepository"/> instances
    /// </summary>
    public interface IRepositoryPlugin
        : IPlugin
    {

        /// <summary>
        /// Creates a new <see cref="IRepository"/>
        /// </summary>
        /// <param name="entityType">The type of entity to create a new <see cref="IRepository"/> for</param>
        /// <param name="keyType">The type of key used to manage the entities to create a new <see cref="IRepository"/> for</param>
        /// <returns>A new <see cref="IRepository"/></returns>
        IRepository CreateRepository(Type entityType, Type keyType);

    }

}
