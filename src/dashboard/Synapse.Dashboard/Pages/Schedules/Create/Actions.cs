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

using Synapse.Integration.Commands.Schedules;
using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Schedules.Create
{

    /// <summary>
    /// Represents the Flux action used to create a new <see cref="V1Schedule"/>
    /// </summary>
    public class CreateSchedule
    {

        /// <summary>
        /// Initializes a new <see cref="CreateSchedule"/>
        /// </summary>
        /// <param name="command">The command to execute</param>
        public CreateSchedule(V1CreateScheduleCommand command)
        {
            this.Command = command;
        }

        /// <summary>
        /// Gets the command to execute
        /// </summary>
        public V1CreateScheduleCommand Command { get; }

    }

    /// <summary>
    /// Represents a Flux action used to handle the differed result of a <see cref="CreateSchedule"/> action
    /// </summary>
    public class HandleCreateScheduleResult
    {

        /// <summary>
        /// Initializes a new <see cref="HandleCreateScheduleResult"/>
        /// </summary>
        /// <param name="schedule">The newly created <see cref="V1Schedule"/></param>
        public HandleCreateScheduleResult(V1Schedule schedule)
        {
            this.Schedule = schedule;
        }

        /// <summary>
        /// Initializes a new <see cref="HandleCreateScheduleResult"/>
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/> that occured during the <see cref="V1Schedule"/>'s creation</param>
        public HandleCreateScheduleResult(Exception exception)
        {
            this.Exception = exception;
        }

        /// <summary>
        /// Gets the newly created <see cref="V1Schedule"/>
        /// </summary>
        public V1Schedule? Schedule { get; }

        /// <summary>
        /// Gets the <see cref="System.Exception"/> that occured during the <see cref="V1Schedule"/>'s creation
        /// </summary>
        public Exception? Exception { get; }

    }

}
