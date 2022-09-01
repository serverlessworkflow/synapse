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

using System.Text.RegularExpressions;

namespace Synapse.Integration.Models
{
    public partial class V1EventFilter
    {

        /// <summary>
        /// Determines whether or not the <see cref="V1EventFilter"/> filters the specified <see cref="V1Event"/>
        /// </summary>
        /// <param name="e">The <see cref="V1Event"/> to filter</param>
        /// <returns>A boolean indicating whether or not the <see cref="V1EventFilter"/> filters the specified <see cref="V1Event"/></returns>
        public virtual bool Filters(V1Event e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            foreach (var attribute in this.Attributes)
            {
                if (!e.TryGetAttribute(attribute.Key, out var value))
                    return false;
                if (!Regex.IsMatch(value, attribute.Value, RegexOptions.IgnoreCase))
                    return false;
            }
            return true;
        }

    }

}
