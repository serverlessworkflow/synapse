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

using Microsoft.AspNetCore.Components;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents the state of an accordion component
    /// </summary>
    public interface IAccordionModel
    {
        /// <summary>
        /// Gets/sets the header of the accordion
        /// </summary>
        public RenderFragment Header { get; set; }
        /// <summary>
        /// Gets/sets the content of the accordion
        /// </summary>
        public RenderFragment Body { get; set; }
        /// <summary>
        /// Gets/sets if the accordion is opened
        /// </summary>
        public bool IsExpanded { get; set; }
        /// <summary>
        /// Gets/sets if the accordion can be opened at the same time than others
        /// </summary>
        public bool AllowsMultiple { get; set; }
    }
}
