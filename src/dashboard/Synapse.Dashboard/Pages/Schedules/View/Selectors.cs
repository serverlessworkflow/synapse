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
using System.Reactive.Linq;

namespace Synapse.Dashboard.Pages.Schedules.View
{

    /// <summary>
    /// Defines selectors for the <see cref="ScheduleViewState"/>
    /// </summary>
    public static class ScheduleViewStateSelectors
    {

        /// <summary>
        /// Selects the current schedule
        /// </summary>
        /// <param name="store">The global <see cref="IStore"/></param>
        /// <returns>A new <see cref="IObservable{T}"/></returns>
        public static IObservable<V1Schedule?> SelectCurrentSchedule(IStore store)
        {
            return store.GetFeature<ScheduleViewState>()
                .Select(state => state.Schedule)
                .DistinctUntilChanged();
        }

        /// <summary>
        /// Selects a boolean that indicates whether or not the current <see cref="V1Schedule"/> has been deleted
        /// </summary>
        /// <param name="store">The global <see cref="IStore"/></param>
        /// <returns>A new <see cref="IObservable{T}"/></returns>
        public static IObservable<bool> SelectIsDeleted(IStore store)
        {
            return store.GetFeature<ScheduleViewState>()
                .Select(state => state.IsDeleted)
                .DistinctUntilChanged();
        }

    }

}
