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

namespace Synapse.Dashboard.Pages.Schedules.View
{

    /// <summary>
    /// Defines Flux reducers for <see cref="ScheduleViewState"/>-related actions
    /// </summary>
    [Reducer]
    public static class Reducers
    {

        /// <summary>
        /// Handles the specified <see cref="InitializeState"/>
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="InitializeState"/> action to reduce</param>
        /// <returns>The reduced state</returns>
        public static ScheduleViewState On(ScheduleViewState state, InitializeState action)
        {
            return new();
        }

        /// <summary>
        /// Handles the specified <see cref="HandleGetScheduleByIdResult"/>
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="HandleGetScheduleByIdResult"/> action to reduce</param>
        /// <returns>The reduced state</returns>
        public static ScheduleViewState On(ScheduleViewState state, HandleGetScheduleByIdResult action)
        {
            return state with
            {
                Processing = false,
                Schedule = action.Result
            };
        }

        /// <summary>
        /// Handles the specified <see cref="TriggerSchedule"/>
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="TriggerSchedule"/> action to reduce</param>
        /// <returns>The reduced state</returns>
        public static ScheduleViewState On(ScheduleViewState state, TriggerSchedule action)
        {
            return state with
            {
                Processing = true
            };
        }

        /// <summary>
        /// Handles the specified <see cref="SuspendSchedule"/>
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="SuspendSchedule"/> action to reduce</param>
        /// <returns>The reduced state</returns>
        public static ScheduleViewState On(ScheduleViewState state, SuspendSchedule action)
        {
            return state with
            {
                Processing = true
            };
        }

        /// <summary>
        /// Handles the specified <see cref="ResumeSchedule"/>
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="ResumeSchedule"/> action to reduce</param>
        /// <returns>The reduced state</returns>
        public static ScheduleViewState On(ScheduleViewState state, ResumeSchedule action)
        {
            return state with
            {
                Processing = true
            };
        }

        /// <summary>
        /// Handles the specified <see cref="RetireSchedule"/>
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="RetireSchedule"/> action to reduce</param>
        /// <returns>The reduced state</returns>
        public static ScheduleViewState On(ScheduleViewState state, RetireSchedule action)
        {
            return state with
            {
                Processing = true
            };
        }

        /// <summary>
        /// Handles the specified <see cref="DeleteSchedule"/>
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="DeleteSchedule"/> action to reduce</param>
        /// <returns>The reduced state</returns>
        public static ScheduleViewState On(ScheduleViewState state, DeleteSchedule action)
        {
            return state with
            {
                Processing = true
            };
        }

        /// <summary>
        /// Handles the specified <see cref="HandleScheduleDeleted"/>
        /// </summary>
        /// <param name="state">The state to reduce</param>
        /// <param name="action">The <see cref="HandleScheduleDeleted"/> action to reduce</param>
        /// <returns>The reduced state</returns>
        public static ScheduleViewState On(ScheduleViewState state, HandleScheduleDeleted action)
        {
            return state with
            {
                IsDeleted = true
            };
        }
    }

}
