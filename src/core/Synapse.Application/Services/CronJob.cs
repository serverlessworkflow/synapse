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

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="ICronJob"/> interface
    /// </summary>
    public class CronJob
        : ICronJob
    {

        private bool _Disposed;

        /// <inheritdoc/>
        public event EventHandler Expired = null!;

        /// <summary>
        /// Initializes a new <see cref="CronJob"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="id">The <see cref="CronJob"/>'s id</param>
        /// <param name="cronExpression">The CRON expression that defines the interval at which to run the job</param>
        /// <param name="timeZone">The <see cref="TimeZoneInfo"/> used to determine the next CRON occurence</param>
        /// <param name="job">A <see cref="Func{T, TResult}"/> that represents the job to execute</param>
        /// <param name="validUntil">The date and time until which the <see cref="CronJob"/> is valid</param>
        public CronJob(IServiceProvider serviceProvider, string id, CronExpression cronExpression, TimeZoneInfo timeZone, Func<IServiceProvider, Task> job, DateTimeOffset validUntil)
        {
            this.ServiceProvider = serviceProvider;
            this.Id = id;
            this.CronExpression = cronExpression;
            this.TimeZone = timeZone;
            this.Job = job;
            this.ValidUntil = validUntil == new DateTimeOffset() ? null : validUntil;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc/>
        public string Id { get; }

        /// <summary>
        /// Gets the CRON expression that defines the interval at which to run the job
        /// </summary>
        public CronExpression CronExpression { get; }

        /// <summary>
        /// Gets the <see cref="TimeZoneInfo"/> used to determine the next CRON occurence
        /// </summary>
        public TimeZoneInfo TimeZone { get; }

        /// <summary>
        /// Gets a <see cref="Func{T, TResult}"/> that represents the job to execute
        /// </summary>
        public Func<IServiceProvider, Task> Job { get; }

        /// <summary>
        /// Gets the date and time until which the <see cref="CronJob"/> is valid
        /// </summary>
        public DateTimeOffset? ValidUntil { get; }

        /// <summary>
        /// Gets the <see cref="System.Threading.Timer"/> used to clock the next CRON occurence
        /// </summary>
        protected Timer Timer { get; private set; } = null!;

        /// <inheritdoc/>
        public virtual async Task ScheduleAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.Now;
            var next = this.CronExpression.GetNextOccurrence(DateTimeOffset.Now, this.TimeZone);
            if (!next.HasValue)
                return;
            var delay = next.Value - DateTimeOffset.Now;
            if (delay.TotalMilliseconds <= 0)
                return;
            this.Timer = new(this.OnNextAsync, null, delay, Timeout.InfiniteTimeSpan);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handles the <see cref="Timer"/>'s completion
        /// </summary>
        /// <param name="state">The <see cref="Timer"/>'s state</param>
        protected virtual async void OnNextAsync(object? state)
        {
            await this.Timer.DisposeAsync();
            this.Timer = null!;
            using var scope = this.ServiceProvider.CreateScope();
            await this.Job(scope.ServiceProvider);
            if (this.ValidUntil.HasValue
                && DateTimeOffset.Now >= this.ValidUntil.Value)
                this.Expired?.Invoke(this, new());
            else
                await this.ScheduleAsync();
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
                    if (this.Timer != null)
                        await this.Timer.DisposeAsync();
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
                    if (this.Timer != null)
                        this.Timer.Dispose();
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
