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

using Synapse.Infrastructure;

namespace Synapse
{
    /// <summary>
    /// Defines extensions for <see cref="IRepositoryFactory"/> instances
    /// </summary>
    public static class IRepositoryFactoryExtensions
    {

        /// <summary>
        /// Creates a new <see cref="IRepository"/>
        /// </summary>
        /// <param name="factory">The extended <see cref="IRepositoryFactory"/></param>
        /// <param name="entityType">The type of entity to create the <see cref="IRepository"/> for</param>
        /// <param name="modelType">The type of the application model to create a new <see cref="IRepository"/> for</param>
        /// <returns>A new <see cref="IRepository"/></returns>
        public static IRepository CreateRepository(this IRepositoryFactory factory, Type entityType, ApplicationModelType modelType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));
            var keyType = entityType.GetGenericType(typeof(IIdentifiable<>)).GetGenericArguments()[0];
            return factory.CreateRepository(entityType, keyType, modelType);
        }

    }

}
