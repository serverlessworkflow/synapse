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
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Defines extensions for <see cref="PropertyInfo"/>s
    /// </summary>
    public static class PropertyInfoExtensions
    {

        /// <summary>
        /// Gets the property's display name
        /// </summary>
        /// <param name="property">The <see cref="PropertyInfo"/> to get the display name for</param>
        /// <returns>The property's display name</returns>
        public static string GetDisplayName(this PropertyInfo property)
        {
            var name = null as string;
            if (property.TryGetCustomAttribute(out DisplayAttribute displayAttribute))
                name = displayAttribute.Name;
            if (string.IsNullOrWhiteSpace(name))
                name = property.Name;
            return name;
        }

        /// <summary>
        /// Gets the property's display order
        /// </summary>
        /// <param name="property">The <see cref="PropertyInfo"/> to get the display order for</param>
        /// <returns>The property's display order</returns>
        public static int GetDisplayOrder(this PropertyInfo property)
        {
            int? order = 0;
            if (property.TryGetCustomAttribute(out DisplayAttribute displayAttribute))
                order = displayAttribute.GetOrder();
            if (!order.HasValue)
                order = 1;
            return order.Value;
        }

    }

}
