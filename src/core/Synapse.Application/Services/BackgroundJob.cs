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

namespace Synapse.Application.Services;

/// <summary>
/// Holds information about a scheduled background job
/// </summary>
public class BackgroundJob
    : IDisposable
{

    /// <summary>
    /// Initializes a new <see cref="BackgroundJob"/>
    /// </summary>
    /// <param name="id">The <see cref="BackgroundJob"/>'s id</param>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="logger">The service used to perform logging</param>
    /// <param name="job">A <see cref="Func{T, TResult}"/> that represents the job to execute</param>
    public BackgroundJob(string id, IServiceProvider serviceProvider, ILogger<BackgroundJob> logger, Func<IServiceProvider, Task> job)
    {
        this.Id = id;
        this.ServiceProvider = serviceProvider;
        this.Logger = logger;
        this.Job = job;
    }

    /// <summary>
    /// Gets the <see cref="BackgroundJob"/>'s id
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets a <see cref="Func{T, TResult}"/> that represents the job to execute
    /// </summary>
    public Func<IServiceProvider, Task> Job { get; protected set; }

    /// <summary>
    /// Gets the date and time the <see cref="BackgroundJob"/> has been scheduled at
    /// </summary>
    public DateTimeOffset ScheduledAt { get; protected set; }

    /// <summary>
    /// Gets the <see cref="System.Threading.Timer"/> used to clock the <see cref="BackgroundJob"/>'s occurences
    /// </summary>
    protected Timer Timer { get; set; } = null!;

    /// <summary>
    /// Schedules the <see cref="BackgroundJob"/>
    /// </summary>
    /// <param name="scheduleAt">The date and time at which to schedule the <see cref="BackgroundJob"/></param>
    public virtual void Schedule(DateTimeOffset scheduleAt)
    {
        if (scheduleAt.ToUniversalTime() < DateTimeOffset.UtcNow) throw new ArgumentOutOfRangeException(nameof(scheduleAt), "The specified value cannot be a date in the past");
        this.ScheduledAt = scheduleAt;
        var dueTime = this.ScheduledAt.ToUniversalTime() - DateTimeOffset.UtcNow;
        this.Timer?.Dispose();
        this.Timer = new(OnExecuteJobAsync, null, dueTime, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// Executes the scheduled work
    /// </summary>
    /// <param name="state">The async state passed to the <see cref="Timer"/> upon initialization</param>
    protected virtual async void OnExecuteJobAsync(object? state)
    {
        using var scope = this.ServiceProvider.CreateScope();
        try
        {
            await this.Job(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            this.Logger.LogError("An error occured while performing the scheduled job with id '{jobId}': {ex}", this.Id, ex);
        }
    }

    private bool _disposed;
    /// <summary>
    /// Disposes of the <see cref="BackgroundJob"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="BackgroundJob"/> is being disposed of</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposed)
        {
            if (disposing) this.Timer?.Dispose();
            this._disposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}