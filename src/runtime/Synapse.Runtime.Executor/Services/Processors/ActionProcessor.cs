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

namespace Synapse.Runtime.Executor.Services.Processors
{
    /// <summary>
    /// Represents the base class for all <see cref="IWorkflowActivityProcessor"/>s used to process <see cref="FunctionDefinition"/>s
    /// </summary>
    public abstract class ActionProcessor
        : WorkflowActivityProcessor
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowActivityProcessor"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
        /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
        /// <param name="activity">The <see cref="V1WorkflowActivityDto"/> to process</param>
        /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
        public ActionProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IOptions<ApplicationOptions> options, V1WorkflowActivityDto activity, ActionDefinition action)
            : base(loggerFactory, context, activityProcessorFactory, options, activity)
        {
            this.Action = action;
        }

        /// <summary>
        /// Gets the <see cref="FunctionDefinition"/> to process
        /// </summary>
        protected ActionDefinition Action { get; }

    }

}
