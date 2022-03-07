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

using Synapse.Integration.Events;
using Synapse.Integration.Events.WorkflowActivities;

namespace Synapse.Runtime.Executor.Services.Processors
{

    /// <summary>
    /// Represents the base class for all <see cref="IWorkflowActivityProcessor"/>s that process <see cref="StateDefinition"/>s
    /// </summary>
    public abstract class StateProcessor
        : WorkflowActivityProcessor, IStateProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="StateProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
        /// <param name="state">The <see cref="StateDefinition"/> to process</param>
        protected StateProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, StateDefinition state)
            : base(loggerFactory, context, activityProcessorFactory, options, activity)
        {
            this.State = state;
        }

        /// <summary>
        /// Gets the <see cref="StateDefinition"/> to process
        /// </summary>
        public virtual StateDefinition State { get; }

    }

    /// <summary>
    /// Represents the base class for all <see cref="IWorkflowActivityProcessor"/>s that process <see cref="StateDefinition"/>s
    /// </summary>
    /// <typeparam name="TState">The type of <see cref="StateDefinition"/> to process</typeparam>
    public abstract class StateProcessor<TState>
        : StateProcessor
        where TState : StateDefinition
    {

        /// <inheritdoc/>
        protected StateProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory, IOptions<ApplicationOptions> options, V1WorkflowActivity activity, TState state) 
            : base(loggerFactory, context, activityProcessorFactory, options, activity, state)
        {
        }

        /// <summary>
        /// Gets the <see cref="StateDefinition"/> to process
        /// </summary>
        public virtual new TState State => (TState)base.State;

    }

}
