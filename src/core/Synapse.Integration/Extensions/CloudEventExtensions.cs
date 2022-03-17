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

using CloudNative.CloudEvents;

namespace Synapse
{
    /// <summary>
    /// Defines extensions for <see cref="CloudEvent"/>s
    /// </summary>
    public static class CloudEventExtensions
    {

        /// <summary>
        /// Attempts to get the specified attribute
        /// </summary>
        /// <param name="e">The <see cref="CloudEvent"/> to check</param>
        /// <param name="name">The name of the attribute to get</param>
        /// <param name="value">The value of the attribute, if any</param>
        /// <returns>A boolean indicating whether or not the specified attribute is defined in the specified <see cref="CloudEvent"/></returns>
        public static bool TryGetAttribute(this CloudEvent e, string name, out string value)
        {
            value = null!;
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            var attribute = e.GetAttribute(name);
            if (attribute == null)
                return false;
            value = attribute.Format(e[attribute]);
            return true;
        }

    }

}
