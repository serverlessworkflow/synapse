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

namespace Synapse.Dashboard.Pages.Schedules.Create
{


    /// <summary>
    /// Represents the Flux state of the view used to create a new <see cref="V1Schedule"/>
    /// </summary>
    [Feature]
    public record CreateScheduleState
    {

        /// <summary>
        /// Gets a boolean indicating whether or not the schedule is being created
        /// </summary>
        public bool Creating { get; init; }

        /// <summary>
        /// Gets the newly created <see cref="V1Schedule"/>
        /// </summary>
        public V1Schedule? Schedule { get; init; }

        /// <summary>
        /// Gets the <see cref="Exception"/> that has occured during the <see cref="V1Schedule"/>'s creation
        /// </summary>
        public Exception? Error { get; init; }

    }

}
