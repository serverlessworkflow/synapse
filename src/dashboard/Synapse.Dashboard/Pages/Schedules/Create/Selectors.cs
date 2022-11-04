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

namespace Synapse.Dashboard.Pages.Schedules.Create
{

    /// <summary>
    /// Defines selectors for <see cref="CreateScheduleState"/>s
    /// </summary>
    public static class CreateScheduleStateSelectors
    {

        /// <summary>
        /// Selects the newly created <see cref="V1Schedule"/>
        /// </summary>
        /// <param name="store">The global <see cref="IStore"/></param>
        /// <returns>A new <see cref="IObservable{T}"/></returns>
        public static IObservable<V1Schedule?> SelectCreatedSchedule(IStore store)
        {
            return store.GetFeature<CreateScheduleState>()
                .Select(state => state.Schedule)
                .DistinctUntilChanged();
        }

        /// <summary>
        /// Selects the error that might have occured during the <see cref="V1Schedule"/> creation
        /// </summary>
        /// <param name="store">The global <see cref="IStore"/></param>
        /// <returns>A new <see cref="IObservable{T}"/></returns>
        public static IObservable<Exception?> SelectError(IStore store)
        {
            return store.GetFeature<CreateScheduleState>()
                .Select(state => state.Error)
                .DistinctUntilChanged();
        }

    }

}
