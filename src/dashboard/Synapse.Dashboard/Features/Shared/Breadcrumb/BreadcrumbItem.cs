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

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a breadcrumb element
    /// </summary>
    public class BreadcrumbItem
        : IBreadcrumbItem
    {
        /// <summary>
        /// The displayed label
        /// </summary>
        public string Label { get; init; }

        /// <summary>
        /// The displayed icon, if any
        /// </summary>
        public string? Icon { get; init; }

        /// <summary>
        /// The navigation link
        /// </summary>
        public string Link { get; init; }

        /// <summary>
        /// Initializes a new <see cref="BreadcrumbItem"/> with the provided data
        /// </summary>
        /// <param name="label"></param>
        /// <param name="link"></param>
        /// <param name="icon"></param>
        public BreadcrumbItem(string label, string link, string? icon = null)
        {
            this.Label = label;
            this.Link = link;
            this.Icon = icon;
        }
    }
}
