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

using System.Collections.Concurrent;

namespace Synapse.Application.Services;

/// <summary>
/// Represents the default, in-memory implementation of the <see cref="IBackgroundJobManager"/> interface
/// </summary>
public class InMemoryBackgroundJobManager
    : IBackgroundJobManager
{

    /// <summary>
    /// Initializes a new <see cref="InMemoryBackgroundJobManager"/>
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    public InMemoryBackgroundJobManager(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> used to store active <see cref="BackgroundJob"/>s
    /// </summary>
    protected ConcurrentDictionary<string, BackgroundJob> BackgroundJobs { get; } = new();

    /// <inheritdoc/>
    public virtual Task ScheduleJobAsync(string jobId, Func<IServiceProvider, Task> job, DateTimeOffset scheduleAt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId)) throw new ArgumentNullException(nameof(jobId));
        if (job == null) throw new ArgumentNullException(nameof(job));
        if (scheduleAt.ToUniversalTime() < DateTimeOffset.UtcNow) throw new ArgumentOutOfRangeException(nameof(scheduleAt), "The specified value cannot be a date in the past");
        if(!this.BackgroundJobs.TryGetValue(jobId, out var backgroundJob))
        {
            backgroundJob = ActivatorUtilities.CreateInstance<BackgroundJob>(this.ServiceProvider, jobId, job);
            this.BackgroundJobs.TryAdd(jobId, backgroundJob);
        }
        backgroundJob.Schedule(scheduleAt);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual Task CancelJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId)) throw new ArgumentNullException(nameof(jobId));
        if (!this.BackgroundJobs.TryGetValue(jobId, out var backgroundJob)) throw new NullReferenceException($"Failed to find a background job with the specified id '{jobId}'");
        this.BackgroundJobs.TryRemove(jobId, out backgroundJob);
        backgroundJob?.Dispose();
        return Task.CompletedTask;
    }

}
