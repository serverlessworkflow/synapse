/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Neuroglia;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Defines extensions for <see cref="Type"/>s
    /// </summary>
    public static class TypeExtensions
    {

        /// <summary>
        /// Determines whether the type implements the <see cref="IAsyncEnumerable{T}"/> interface
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>A boolean indicating whether the type implements the <see cref="IAsyncEnumerable{T}"/> interface</returns>
        public static bool IsAsyncEnumerable(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            return type.GetGenericType(typeof(IAsyncEnumerable<>)) != null;
        }

    }

}
