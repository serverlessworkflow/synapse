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

namespace Synapse
{
    /// <summary>
    /// Defines helpers for handling aggregates
    /// </summary>
    public static class AggregateHelper
    {

        /// <summary>
        /// Gets the name of the specified aggregate type
        /// </summary>
        /// <param name="type">The type of the aggregate to get the name of</param>
        /// <returns>The name of the specified aggregate type</returns>
        public static string NameOf(Type type)
        {
            return type.Name[VersionOf(type).Length..].ToLowerInvariant();
        }

        /// <summary>
        /// Gets the name of the specified aggregate type
        /// </summary>
        /// <typeparam name="TAggregate">The type of the aggregate to get the name of</typeparam>
        /// <returns>The name of the specified aggregate type</returns>
        public static string NameOf<TAggregate>()
            where TAggregate : class
        {
            return NameOf(typeof(TAggregate));
        }

        /// <summary>
        /// Gets the version of the specified aggregate type
        /// </summary>
        /// <param name="type">The type of the aggregate to get the version of</param>
        /// <returns>The version of the specified aggregate type</returns>
        public static string VersionOf(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            var components = type.Name.SplitCamelCase(false, false).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return components.First().ToLowerInvariant();
        }

        /// <summary>
        /// Gets the version of the specified aggregate type
        /// </summary>
        /// <typeparam name="TAggregate">The type of the aggregate to get the version of</typeparam>
        /// <returns>The version of the specified aggregate type</returns>
        public static string VersionOf<TAggregate>()
        {
            return VersionOf(typeof(TAggregate));
        }

    }

}
