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
            return activity.Type switch
            {
                V1WorkflowActivityType.Action => this.CreateActionActivityProcessor(state, activity),
                V1WorkflowActivityType.Branch => throw new NotImplementedException(),//todo
                V1WorkflowActivityType.ConsumeEvent => throw new NotImplementedException(),//todo
                V1WorkflowActivityType.End => ActivatorUtilities.CreateInstance<EndProcessor>(this.ServiceProvider, activity, state.End ?? new()),
                V1WorkflowActivityType.Error => throw new NotImplementedException(),//todo
                V1WorkflowActivityType.EventTrigger => throw new NotImplementedException(),//todo
                V1WorkflowActivityType.Function => this.CreateFunctionActivityProcessor(state, activity),
                V1WorkflowActivityType.Iteration => throw new NotImplementedException(),//todo
                V1WorkflowActivityType.ProduceEvent => throw new NotImplementedException(),//todo
                V1WorkflowActivityType.Start => ActivatorUtilities.CreateInstance<StartProcessor>(this.ServiceProvider, activity, this.Context.Workflow.Definition.Start ?? new()),
                V1WorkflowActivityType.State => this.CreateStateActivityProcessor(state, activity),
                V1WorkflowActivityType.SubFlow => throw new NotImplementedException(),//todo
                V1WorkflowActivityType.Transition => throw new NotImplementedException(),//todo
                _ => throw new NotSupportedException($"The specified {typeof(V1WorkflowActivityType).Name} '{activity.Type}' is not supported"),
            };
        }

        protected virtual IStateProcessor CreateStateActivityProcessor(StateDefinition state, V1WorkflowActivityDto activity)
        {
            return state switch
            {
                //CallbackStateDefinition callbackState => ActivatorUtilities.CreateInstance<CallbackStateProcessor>(this.ServiceProvider, state, activity), //todo
                //EventStateDefinition eventState => ActivatorUtilities.CreateInstance<EventStateProcessor>(this.ServiceProvider, state, activity),
                //ForEachStateDefinition forEachState => ActivatorUtilities.CreateInstance<ForEachStateProcessor>(this.ServiceProvider, state, activity),//todo
                InjectStateDefinition injectState => ActivatorUtilities.CreateInstance<InjectStateProcessor>(this.ServiceProvider, state, activity),
                OperationStateDefinition operationState => ActivatorUtilities.CreateInstance<OperationStateProcessor>(this.ServiceProvider, state, activity),
                //ParallelStateDefinition parallelState => ActivatorUtilities.CreateInstance<ParallelStateProcessor>(this.ServiceProvider, state, activity),//todo
                //SleepStateDefinition delayState => ActivatorUtilities.CreateInstance<DelayStateProcessor>(this.ServiceProvider, state, activity),//todo
                //SwitchStateDefinition switchState => ActivatorUtilities.CreateInstance<SwitchStateProcessor>(this.ServiceProvider, state, activity),//todo
                _ => throw new NotSupportedException($"The specified {nameof(StateDefinition)} type '{state.GetType().Name}' is not supported"),
            };
        }

        protected virtual IWorkflowActivityProcessor CreateActionActivityProcessor(StateDefinition state, V1WorkflowActivityDto activity)
        {
            if(!state.TryGetAction(activity.Metadata, out var action))
                throw new NullReferenceException($"Failed to find an action that matches the metadata specified by the activity with id '{activity.Id}'");
            return ActivatorUtilities.CreateInstance<ActionProcessor>(this.ServiceProvider, activity, action);
        }

        protected virtual IWorkflowActivityProcessor CreateFunctionActivityProcessor(StateDefinition state, V1WorkflowActivityDto activity)
        {
            if (!state.TryGetAction(activity.Metadata, out var action))
                throw new NullReferenceException($"Failed to find an action that matches the metadata specified by the activity with id '{activity.Id}'");
            if (!this.Context.Workflow.Definition.TryGetFunction(action.Function!.RefName, out FunctionDefinition function))
                throw new NullReferenceException($"Failed to find a function with the specified name '{action.Function.RefName}' in the workflow with name '{this.Context.Workflow.Definition.Id}' and version '{this.Context.Workflow.Definition.Version}'");
            switch (function.Type)
            {
                //case FunctionType.AsyncApi://todo
                //    break;
                //case FunctionType.Expression:
                //    return ActivatorUtilities.CreateInstance<ExpressionFunctionProcessor>(this.ServiceProvider, state, activity, action, function);
                //case FunctionType.GraphQL:
                //    return ActivatorUtilities.CreateInstance<GraphQLFunctionProcessor>(this.ServiceProvider, state, activity, action, function);
                case FunctionType.OData:
                    return ActivatorUtilities.CreateInstance<ODataFunctionProcessor>(this.ServiceProvider, activity, action, function);
                //case FunctionType.OpenApi://todo
                //    break;
                case FunctionType.Rest:
                    return ActivatorUtilities.CreateInstance<OpenApiFunctionProcessor>(this.ServiceProvider, activity, action, function);
                //case FunctionType.Rpc:
                //    return ActivatorUtilities.CreateInstance<RpcFunctionProcessor>(this.ServiceProvider, state, activity, action, function);
                default:
                    throw new NotSupportedException($"The specified {nameof(FunctionType)} '{function.Type}' is not supported");
            }
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
