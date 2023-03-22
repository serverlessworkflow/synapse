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

namespace Synapse.Worker.Services.Processors
{

    /// <summary>
    /// Represents the base class for all <see cref="IWorkflowActivityProcessor"/>s used to process <see cref="TransitionDefinition"/>s
    /// </summary>
    public class TransitionProcessor
        : WorkflowActivityProcessor, ITransitionProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="TransitionProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="state">The <see cref="StateDefinition"/> to process the transition of</param>
        /// <param name="transition">The <see cref="TransitionDefinition"/> to process</param>
        public TransitionProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IOptions<ApplicationOptions> options, V1WorkflowActivity activity, StateDefinition state, TransitionDefinition transition)
            : base(loggerFactory, context, activityProcessorFactory, options, activity)
        {
            this.State = state;
            this.Transition = transition;
        }

        /// <summary>
        /// Gets the <see cref="StateDefinition"/> to process the transition of
        /// </summary>
        public StateDefinition State { get; }

        /// <summary>
        /// Gets the <see cref="TransitionDefinition"/> to process
        /// </summary>
        public TransitionDefinition Transition { get; }

        /// <inheritdoc/>
        protected override Task InitializeAsync(CancellationToken cancellationToken)
        {
            //TODO
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            //TODO
            await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, this.Activity.Input!.ToObject()!), cancellationToken);
            await this.OnCompletedAsync(cancellationToken);
        }

    }

}
