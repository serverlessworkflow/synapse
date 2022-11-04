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

using Neuroglia.Data.Flux;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Schedules.View
{

    /// <summary>
    /// Represents the Flux state used by the schedule view
    /// </summary>
    [Feature]
    public record ScheduleViewState
    {

        /// <summary>
        /// Gets a boolean indicating whether or not the application is processing
        /// </summary>
        public bool Processing { get; set; }

        /// <summary>
        /// Gets a boolean indicating whether or not the current <see cref="V1Schedule"/> has been deleted
        /// </summary>
        public bool IsDeleted { get; init; }

        /// <summary>
        /// Gets the current <see cref="V1Schedule"/>, if any
        /// </summary>
        public V1Schedule? Schedule { get; init; }

    }

}
