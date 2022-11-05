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

namespace Synapse.Infrastructure.Services;

/// <summary>
/// Defines the fundamentals of a service used to manage background jobs
/// </summary>
public interface IBackgroundJobManager
{

    /// <summary>
    /// Schedules a new background job
    /// </summary>
    /// <param name="jobId">The unique id of the job to schedule</param>
    /// <param name="job">A <see cref="Func{T, TResult}"/> representing the job to schedule</param>
    /// <param name="scheduleAt">The date and time at which to schedule the job</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task ScheduleJobAsync(string jobId, Func<IServiceProvider, Task> job, DateTimeOffset scheduleAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels the specified background job
    /// </summary>
    /// <param name="jobId">The id of the background job to cancel</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task CancelJobAsync(string jobId, CancellationToken cancellationToken = default);

}

