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

using Synapse.Infrastructure.Plugins;

namespace Synapse.Application.Services
{

    /// <summary>
    /// Represents a <see cref="BackgroundService"/> used to schedule all CRON-based <see cref="V1Workflow"/>s at startup
    /// </summary>
    public class WorkflowScheduler
        : BackgroundService
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowScheduler"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="pluginManager">The service used to manage <see cref="IPlugin"/>s</param>
        /// <param name="backgroundJobManager">The service used to manage background jobs</param>
        public WorkflowScheduler(IServiceProvider serviceProvider, ILogger<WorkflowScheduler> logger, IPluginManager pluginManager, IBackgroundJobManager backgroundJobManager)
        {
            this.ServiceProvider = serviceProvider;
            this.Logger = logger;
            this.PluginManager = pluginManager;
            this.BackgroundJobManager = backgroundJobManager;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to manage <see cref="IPlugin"/>s
        /// </summary>
        protected IPluginManager PluginManager { get; }

        /// <summary>
        /// Gets the service used to manage background jobs
        /// </summary>
        protected IBackgroundJobManager BackgroundJobManager { get; }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.PluginManager.WaitForStartupAsync(stoppingToken);
            using var scope = this.ServiceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var schedules = scope.ServiceProvider.GetRequiredService<IRepository<Integration.Models.V1Schedule>>();
            foreach(var schedule in schedules.AsQueryable()
                .Where(s => s.Status == V1ScheduleStatus.Active)
                .ToList())
            {
                await this.BackgroundJobManager.ScheduleJobAsync(schedule, stoppingToken);
            }
        }

    }

}