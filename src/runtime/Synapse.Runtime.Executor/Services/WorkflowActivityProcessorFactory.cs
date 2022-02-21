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

using Microsoft.Extensions.DependencyInjection;
using Synapse.Runtime.Executor.Services.Processors;

namespace Synapse.Runtime.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowActivityProcessorFactory"/> interface
    /// </summary>
    public class WorkflowActivityProcessorFactory
        : IWorkflowActivityProcessorFactory
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowActivityProcessorFactory"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        public WorkflowActivityProcessorFactory(IServiceProvider serviceProvider, IWorkflowRuntimeContext context)
        {
            this.ServiceProvider = serviceProvider;
            this.Context = context;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the current <see cref="IWorkflowExecutionContext"/>
        /// </summary>
        protected IWorkflowRuntimeContext Context { get; }

        /// <inheritdoc/>
        public virtual IWorkflowActivityProcessor Create(V1WorkflowActivityDto activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (!activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.State, out var stateName))
                throw new ArgumentException($"The specified activity '{activity.Id}' is missing the required metadata field '{V1WorkflowActivityMetadata.State}'");
            if (!this.Context.Workflow.Definition.TryGetState(stateName, out StateDefinition state))
                throw new NullReferenceException($"Failed to find the workflow state with the specified name '{stateName}'");
            switch (activity.Type)
            {
                case V1WorkflowActivityType.Action:

                    break;
                case V1WorkflowActivityType.Branch:

                    break;
                case V1WorkflowActivityType.ConsumeEvent:

                    break;
                case V1WorkflowActivityType.End:
                    return ActivatorUtilities.CreateInstance<EndProcessor>(this.ServiceProvider, activity);
                case V1WorkflowActivityType.Error:

                    break;
                case V1WorkflowActivityType.EventTrigger:

                    break;
                case V1WorkflowActivityType.Function:

                    break;
                case V1WorkflowActivityType.Iteration:

                    break;
                case V1WorkflowActivityType.ProduceEvent:

                    break;
                case V1WorkflowActivityType.Start:

                    break;
                case V1WorkflowActivityType.State:
                    return state switch
                    {
                        //case CallbackStateDefinition callbackState:
                        //    return ActivatorUtilities.CreateInstance<CallbackStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        //case DelayStateDefinition delayState:
                        //    return ActivatorUtilities.CreateInstance<DelayStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        //case EventStateDefinition eventState:
                        //    return ActivatorUtilities.CreateInstance<EventStateProcessor>(this.ServiceProvider, state, activity);
                        //case ForEachStateDefinition forEachState:
                        //    return ActivatorUtilities.CreateInstance<ForEachStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        InjectStateDefinition injectState => ActivatorUtilities.CreateInstance<InjectStateProcessor>(this.ServiceProvider, state, activity),
                        //case OperationStateDefinition operationState:
                        //    return ActivatorUtilities.CreateInstance<OperationStateProcessor>(this.ServiceProvider, state, activity);
                        //case ParallelStateDefinition parallelState:
                        //    return ActivatorUtilities.CreateInstance<ParallelStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        //case SubFlowStateDefinition subFlowState:
                        //    return ActivatorUtilities.CreateInstance<SubFlowStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        //case SwitchStateDefinition switchState:
                        //    return ActivatorUtilities.CreateInstance<SwitchStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        _ => throw new NotSupportedException($"The specified {nameof(StateDefinition)} type '{state.GetType().Name}' is not supported"),
                    };
                case V1WorkflowActivityType.SubFlow:

                    break;
                case V1WorkflowActivityType.Transition:

                    break;
                default:
                    throw new NotSupportedException($"The specified {typeof(V1WorkflowActivityType).Name} '{activity.Type}' is not supported");
            }
            return null; //todo: throw exception
        }

        IWorkflowActivityProcessor IWorkflowActivityProcessorFactory.Create(V1WorkflowActivityDto activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            return this.Create(activity);
        }

        IWorkflowActivityProcessor<TActivity> IWorkflowActivityProcessorFactory.Create<TActivity>(TActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            return (IWorkflowActivityProcessor<TActivity>)this.Create(activity);
        }

        TProcessor IWorkflowActivityProcessorFactory.Create<TProcessor>(V1WorkflowActivityDto activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            return (TProcessor)this.Create(activity);
        }

        TProcessor IWorkflowActivityProcessorFactory.Create<TProcessor, TActivity>(TActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            return (TProcessor)this.Create(activity);
        }

    }

}
