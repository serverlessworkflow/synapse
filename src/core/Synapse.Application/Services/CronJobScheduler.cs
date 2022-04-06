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
using System.Collections.Concurrent;

namespace Synapse.Application.Services
{
    /// <summary>
    /// Represents the default implementation of the <see cref="ICronJobScheduler"/> interface
    /// </summary>
    public class CronJobScheduler
        : ICronJobScheduler
    {

        private bool _Disposed;

        /// <summary>
        /// Initializes a new <see cref="CronJobScheduler"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        public CronJobScheduler(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> containing all registered <see cref="ICronJob"/>s
        /// </summary>
        protected ConcurrentDictionary<string, ICronJob> Jobs { get; } = new();

        /// <inheritdoc/>
        public virtual async Task ScheduleJobAsync(string jobId, CronExpression cronExpression, TimeZoneInfo timeZone, Func<IServiceProvider, Task> job, DateTimeOffset? validUntil = null, CancellationToken cancellationToken = default)
        {
            if(cronExpression == null)
                throw new ArgumentNullException(nameof(cronExpression));
            if (timeZone == null)
                throw new ArgumentNullException(nameof(timeZone));
            if (job == null)
                throw new ArgumentNullException(nameof(job));
            var cronJob = ActivatorUtilities.CreateInstance<CronJob>(this.ServiceProvider, jobId, cronExpression, timeZone, job, validUntil.HasValue ? validUntil.Value : new());
            cronJob.Expired += async (sender, e) => await this.OnJobExpiredAsync((ICronJob)sender!);
            this.Jobs.AddOrUpdate(jobId, cronJob, (key, current) =>
            {
                current.Dispose();
                return cronJob;
            });
            await cronJob.ScheduleAsync(cancellationToken);
        }

        /// <summary>
        /// Handles the specified <see cref="ICronJob"/>'s expiration
        /// </summary>
        /// <param name="cronJob">The expired <see cref="ICronJob"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnJobExpiredAsync(ICronJob cronJob)
        {
            this.Jobs.TryRemove(cronJob.Id, out _);
            await cronJob.DisposeAsync();
        }

        /// <summary>
        /// Disposes of the <see cref="CronJob"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="CronJob"/> is being disposed of</param>
        /// <returns>A new awaitable <see cref="ValueTask"/></returns>
        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    foreach(var job in this.Jobs.Values)
                    {
                        await job.DisposeAsync();
                    }
                    this.Jobs.Clear();
                }
                this._Disposed = true;
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsync(disposing: true);
            GC.SuppressFinalize(this);

        }

        /// <summary>
        /// Disposes of the <see cref="CronJob"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="CronJob"/> is being disposed of</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    foreach (var job in this.Jobs.Values)
                    {
                        job.Dispose();
                    }
                    this.Jobs.Clear();
                }
                this._Disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }

}