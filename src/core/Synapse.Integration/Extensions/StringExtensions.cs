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
    /// Defines extensions for strings
    /// </summary>
    public static class StringExtensions
    {

        /// <summary>
        /// Determines whether or not the string is a workflow expression
        /// </summary>
        /// <param name="text">The string to test</param>
        /// <returns>A boolean indicating whether or not the string is a workflow expression</returns>
        public static bool IsRuntimeExpression(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            return text.StartsWith("${") && text.EndsWith("}");
        }

    }

}
