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

using Microsoft.Extensions.Logging;

namespace Synapse.Infrastructure.Plugins
{
    /// <summary>
    /// Represents a base class for <see cref="IPlugin"/> implementations
    /// </summary>
    public abstract class Plugin
        : IPlugin
    {

        private bool _Disposed;

        /// <summary>
        /// Initializes a new <see cref="Plugin"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        protected Plugin(ILoggerFactory loggerFactory)
        {
            this.Logger = loggerFactory.CreateLogger(this.GetType());
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the <see cref="Plugin"/>'s <see cref="System.Threading.CancellationTokenSource"/>
        /// </summary>
        protected CancellationTokenSource CancellationTokenSource { get; private set; } = null!;

        /// <inheritdoc/>
        async ValueTask IPlugin.InitializeAsync(CancellationToken stoppingToken)
        {
            this.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            await this.InitializeAsync(this.CancellationTokenSource.Token);
        }

        /// <summary>
        /// Initialzes the <see cref="Plugin"/>
        /// </summary>
        /// <param name="stoppingToken">A <see cref="CancellationToken"/> used to manage the <see cref="Plugin"/>'s lifetime</param>
        /// <returns>A new awaitable <see cref="ValueTask"/></returns>
        protected virtual ValueTask InitializeAsync(CancellationToken stoppingToken)
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Disposes of the <see cref="Plugin"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="Plugin"/> is being disposed of</param>
        /// <returns>A new awaitable <see cref="ValueTask"/></returns>
        protected virtual ValueTask DisposeAsync(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    
                }
                this._Disposed = true;
            }
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the <see cref="Plugin"/>
        /// </summary>
        /// <param name="disposing">A boolean indicating whether or not the <see cref="Plugin"/> is being disposed of</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                if (disposing)
                {
                    this.CancellationTokenSource?.Dispose();
                }
                this._Disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

}
