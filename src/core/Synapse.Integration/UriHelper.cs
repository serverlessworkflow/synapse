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
    /// Defines methods to handle <see cref="Uri"/>
    /// </summary>
    public static class UriHelper
    {

        /// <summary>
        /// Combines the specified base <see cref="Uri"/> with a relative uri string
        /// </summary>
        /// <param name="baseUri">The base <see cref="Uri"/> to combine to the specified relative uri</param>
        /// <param name="relativeUri">The relative <see cref="Uri"/> to combine to the specified base uri</param>
        /// <returns>A new <see cref="Uri"/>, result of the combination of both the base and relative uris</returns>
        public static Uri Combine(Uri baseUri, string relativeUri)
        {
            if (baseUri == null)
                throw new ArgumentNullException(nameof(baseUri));
            if (string.IsNullOrWhiteSpace(relativeUri))
                return baseUri;
            return new($"{baseUri.ToString().TrimEnd('/')}/{relativeUri.TrimStart('/')}");
        }

        /// <summary>
        /// Combines the specified base <see cref="Uri"/> with an array of relative paths
        /// </summary>
        /// <param name="baseUri">The base <see cref="Uri"/> to combine to the specified relative paths</param>
        /// <param name="relativePaths">An array containing the relative paths to combine to the specified base uri</param>
        /// <returns>A new <see cref="Uri"/>, result of the combination of both the base and relative paths</returns>
        public static Uri Combine(Uri baseUri, params string[] relativePaths)
        {
            if (baseUri == null)
                throw new ArgumentNullException(nameof(baseUri));
            if (relativePaths.Length == 0)
                return baseUri;
            var currentUrl = Combine(baseUri, relativePaths[0]);
            return Combine(currentUrl, relativePaths.Skip(1).ToArray());
        }

    }

}
