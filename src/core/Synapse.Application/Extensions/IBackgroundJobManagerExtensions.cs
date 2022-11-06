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

using Synapse.Application.Commands.Schedules;

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="IBackgroundJobManager"/>s
    /// </summary>
    public static class IBackgroundJobManagerExtensions
    {

        /// <summary>
        /// Schedules a new background job
        /// </summary>
        /// <param name="backgroundJobManager">The service used to manage background jobs</param>
        /// <param name="schedule">An object that defines the background job to schedule</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task ScheduleJobAsync(this IBackgroundJobManager backgroundJobManager, V1Schedule schedule, CancellationToken cancellationToken = default)
        {
            if (schedule == null) throw new ArgumentNullException(nameof(schedule));
            if (!schedule.NextOccurenceAt.HasValue) throw new ArgumentException("The specified schedule does not define a next occurence", nameof(schedule));
            await backgroundJobManager.ScheduleJobAsync(schedule.Id, async provider => await OnInstanciateWorkflowAsync(provider, schedule.Id), schedule.NextOccurenceAt.Value, cancellationToken);
        }

        /// <summary>
        /// Schedules a new background job
        /// </summary>
        /// <param name="backgroundJobManager">The service used to manage background jobs</param>
        /// <param name="schedule">An object that defines the background job to schedule</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task ScheduleJobAsync(this IBackgroundJobManager backgroundJobManager, Integration.Models.V1Schedule schedule, CancellationToken cancellationToken = default)
        {
            if (schedule == null) throw new ArgumentNullException(nameof(schedule));
            if (!schedule.NextOccurenceAt.HasValue) throw new ArgumentException("The specified schedule does not define a next occurence", nameof(schedule));
            await backgroundJobManager.ScheduleJobAsync(schedule.Id, async provider => await OnInstanciateWorkflowAsync(provider, schedule.Id), schedule.NextOccurenceAt.Value, cancellationToken);
        }

        private static async Task OnInstanciateWorkflowAsync(IServiceProvider serviceProvider, string scheduleId)
        {
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            await mediator.ExecuteAndUnwrapAsync(new V1TriggerScheduleCommand(scheduleId));
        }

    }

}
