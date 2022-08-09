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

using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Worker.Services.Processors
{

    /// <summary>
    /// Represents a <see cref="IWorkflowActivityProcessor"/> used to process <see cref="SleepStateDefinition"/>s
    /// </summary>
    public class SleepStateProcessor
        : StateProcessor<SleepStateDefinition>
    {

        /// <inheritdoc/>
        public SleepStateProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory, 
            IOptions<ApplicationOptions> options, V1WorkflowActivity activity, SleepStateDefinition state) 
            : base(loggerFactory, context, activityProcessorFactory, options, activity, state)
        {

        }

        /// <summary>
        /// Gets the sleep duration
        /// </summary>
        protected TimeSpan Duration { get; private set; }

        /// <summary>
        /// Gets the <see cref="System.Threading.Timer"/> used to delay the <see cref="SleepStateProcessor"/>'s execution
        /// </summary>
        protected Timer Timer { get; set; } = null!;

        /// <inheritdoc/>
        protected override Task InitializeAsync(CancellationToken cancellationToken)
        {
            if(this.Activity.Status == V1WorkflowActivityStatus.Suspended)
                this.Duration = this.State.Duration.Subtract(this.Context.Workflow.Instance.Sessions.Last(s => !s.IsActive).EndedAt!.Value.Subtract(this.Activity.StartedAt!.Value));
            else
                this.Duration = this.State.Duration;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task ProcessAsync(CancellationToken cancellationToken)
        {
            var duration = this.Duration; //todo: substract activity's total execution time
            this.Timer = new(async (state) => await this.OnDelayElapsedAsync(cancellationToken), duration, duration, Timeout.InfiniteTimeSpan);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the event fired when the <see cref="SleepStateProcessor"/>'s <see cref="System.Threading.Timer"/> has elapsed
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns></returns>
        protected virtual async Task OnDelayElapsedAsync(CancellationToken cancellationToken)
        {
            await this.Timer.DisposeAsync();
            this.Timer = null!;
            await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, this.Activity.Input), cancellationToken);
            await this.OnCompletedAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.Timer?.Dispose();
            base.Dispose(disposing);
        }

    }

}
