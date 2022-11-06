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

using Synapse.Integration.Models;

namespace Synapse.Dashboard.Pages.Schedules.View
{

    /// <summary>
    /// Represents the Flux action used to initialize the <see cref="ScheduleViewState"/>
    /// </summary>
    public class InitializeState
    {



    }

    /// <summary>
    /// Represents the Flux action used to get a <see cref="V1Schedule"/> by id
    /// </summary>
    public class GetScheduleById
    {

        /// <summary>
        /// Initializes a new <see cref="GetScheduleById"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Schedule"/> to get</param>
        public GetScheduleById(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Schedule"/> to get
        /// </summary>
        public string Id { get; }

    }

    /// <summary>
    /// Repesents the Flux action used to handle the differed result of a <see cref="GetScheduleById"/> Flux action
    /// </summary>
    public class HandleGetScheduleByIdResult
    {

        /// <summary>
        /// Initializes a new <see cref="HandleGetScheduleByIdResult"/>
        /// </summary>
        /// <param name="result">The differed result of a <see cref="GetScheduleById"/> Flux action</param>
        public HandleGetScheduleByIdResult(V1Schedule? result)
        {
            this.Result = result;
        }

        /// <summary>
        /// Gets the differed result of a <see cref="GetScheduleById"/> Flux action
        /// </summary>
        public V1Schedule? Result { get; }

    }

    /// <summary>
    /// Represents the Flux action used to trigger a <see cref="V1Schedule"/>
    /// </summary>
    public class TriggerSchedule
    {

        /// <summary>
        /// Initializes a new <see cref="TriggerSchedule"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Schedule"/> to trigger</param>
        public TriggerSchedule(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Schedule"/> to trigger
        /// </summary>
        public string Id { get; }

    }

    /// <summary>
    /// Represents the Flux action used to suspend a <see cref="V1Schedule"/>
    /// </summary>
    public class SuspendSchedule
    {

        /// <summary>
        /// Initializes a new <see cref="SuspendSchedule"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Schedule"/> to suspend</param>
        public SuspendSchedule(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Schedule"/> to suspend
        /// </summary>
        public string Id { get; }

    }

    /// <summary>
    /// Represents the Flux action used to resume a <see cref="V1Schedule"/>
    /// </summary>
    public class ResumeSchedule
    {

        /// <summary>
        /// Initializes a new <see cref="ResumeSchedule"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Schedule"/> to resume</param>
        public ResumeSchedule(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Schedule"/> to resume
        /// </summary>
        public string Id { get; }

    }

    /// <summary>
    /// Represents the Flux action used to retire a <see cref="V1Schedule"/>
    /// </summary>
    public class RetireSchedule
    {

        /// <summary>
        /// Initializes a new <see cref="RetireSchedule"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Schedule"/> to retire</param>
        public RetireSchedule(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Schedule"/> to retire
        /// </summary>
        public string Id { get; }

    }

    /// <summary>
    /// Represents the Flux action used to delete a <see cref="V1Schedule"/>
    /// </summary>
    public class DeleteSchedule
    {

        /// <summary>
        /// Initializes a new <see cref="DeleteSchedule"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Schedule"/> to delete</param>
        public DeleteSchedule(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the <see cref="V1Schedule"/> to delete
        /// </summary>
        public string Id { get; }

    }

    /// <summary>
    /// Represents the Flux action used to handle the deletion of a <see cref="V1Schedule"/>
    /// </summary>
    public class HandleScheduleDeleted
    {


    }

}