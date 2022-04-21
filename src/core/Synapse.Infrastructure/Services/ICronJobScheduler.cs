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

using Cronos;

namespace Synapse.Infrastructure.Services
{

    /// <summary>
    /// Defines the fundamentals of a service used to schedule CRON jobs
    /// </summary>
    public interface ICronJobScheduler
        : IDisposable, IAsyncDisposable
    {

        /// <summary>
        /// Schedules the specified job
        /// </summary>
        /// <param name="jobId">The id of the job to schedule</param>
        /// <param name="cronExpression">The <see cref="CronExpression"/> that represents the interval at which to run the job</param>
        /// <param name="timeZone">The <see cref="TimeZoneInfo"/> used to determine the next CRON occurence</param>
        /// <param name="job">The job to execute</param>
        /// <param name="validUntil">The date and time until which the job to schedule is valid</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task ScheduleJobAsync(string jobId, CronExpression cronExpression, TimeZoneInfo timeZone, Func<IServiceProvider, Task> job, DateTimeOffset? validUntil = null, CancellationToken cancellationToken = default);

    }

}
