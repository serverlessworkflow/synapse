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
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
        public WorkflowActivityProcessorFactory(IServiceProvider serviceProvider, ILogger<WorkflowActivityProcessorFactory> logger, IWorkflowRuntimeContext context)
        {
            this.ServiceProvider = serviceProvider;
            this.Logger = logger;
            this.Context = context;
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
        /// Gets the current <see cref="IWorkflowExecutionContext"/>
        /// </summary>
        protected IWorkflowRuntimeContext Context { get; }

        /// <inheritdoc/>
        public virtual IWorkflowActivityProcessor Create(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (!activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.State, out var stateName))
                throw new ArgumentException($"The specified activity '{activity.Id}' is missing the required metadata field '{V1WorkflowActivityMetadata.State}'");
            if (!this.Context.Workflow.Definition.TryGetState(stateName, out var state))
                throw new NullReferenceException($"Failed to find the workflow state with the specified name '{stateName}'");
            try
            {
                return activity.Type switch
                {
                    V1WorkflowActivityType.Action => this.CreateActionActivityProcessor(state, activity),
                    V1WorkflowActivityType.Branch => throw new NotImplementedException(),//todo
                    V1WorkflowActivityType.ConsumeEvent => this.CreateConsumeEventActivityProcessor(activity),
                    V1WorkflowActivityType.End => ActivatorUtilities.CreateInstance<EndProcessor>(this.ServiceProvider, activity, state.End ?? new()),
                    V1WorkflowActivityType.Error => throw new NotImplementedException(),//todo
                    V1WorkflowActivityType.EventTrigger => this.CreateEventStateTriggerProcessor(state, activity),
                    V1WorkflowActivityType.Iteration => throw new NotImplementedException(),//todo
                    V1WorkflowActivityType.ProduceEvent => this.CreateProduceEventActivityProcessor(activity),
                    V1WorkflowActivityType.Start => ActivatorUtilities.CreateInstance<StartProcessor>(this.ServiceProvider, activity, this.Context.Workflow.Definition.Start ?? new()),
                    V1WorkflowActivityType.State => this.CreateStateActivityProcessor(state, activity),
                    V1WorkflowActivityType.SubFlow => throw new NotImplementedException(),//todo
                    V1WorkflowActivityType.Transition => throw new NotImplementedException(),//todo
                    _ => throw new NotSupportedException($"The specified {typeof(V1WorkflowActivityType).Name} '{activity.Type}' is not supported"),
                };
            }
            catch(Exception ex)
            {
                this.Logger.LogError("An error occured while creating a new processor for the activity with id '{activityId}': {ex}", activity.Id, ex.ToString());
                throw;
            }
        }

        protected virtual IStateProcessor CreateStateActivityProcessor(StateDefinition state, V1WorkflowActivity activity)
        {
            return state switch
            {
                //CallbackStateDefinition callbackState => ActivatorUtilities.CreateInstance<CallbackStateProcessor>(this.ServiceProvider, state, activity), //todo
                EventStateDefinition eventState => ActivatorUtilities.CreateInstance<EventStateProcessor>(this.ServiceProvider, state, activity),
                //ForEachStateDefinition forEachState => ActivatorUtilities.CreateInstance<ForEachStateProcessor>(this.ServiceProvider, state, activity),//todo
                InjectStateDefinition injectState => ActivatorUtilities.CreateInstance<InjectStateProcessor>(this.ServiceProvider, state, activity),
                OperationStateDefinition operationState => ActivatorUtilities.CreateInstance<OperationStateProcessor>(this.ServiceProvider, state, activity),
                //ParallelStateDefinition parallelState => ActivatorUtilities.CreateInstance<ParallelStateProcessor>(this.ServiceProvider, state, activity),//todo
                //SleepStateDefinition delayState => ActivatorUtilities.CreateInstance<DelayStateProcessor>(this.ServiceProvider, state, activity),//todo
                //SwitchStateDefinition switchState => ActivatorUtilities.CreateInstance<SwitchStateProcessor>(this.ServiceProvider, state, activity),//todo
                _ => throw new NotSupportedException($"The specified {nameof(StateDefinition)} type '{state.GetType().Name}' is not supported"),
            };
        }

        protected virtual IWorkflowActivityProcessor CreateActionActivityProcessor(StateDefinition state, V1WorkflowActivity activity)
        {
            if(!state.TryGetAction(activity.Metadata, out var action))
                throw new NullReferenceException($"Failed to find an action that matches the metadata specified by the activity with id '{activity.Id}'");
            return action.Type switch
            {
                ActionType.Function => this.CreateFunctionActivityProcessor(state, activity),
                ActionType.Subflow => throw new NotImplementedException(),//todo
                ActionType.Trigger => this.CreateAsyncFunctionActivityProcessor(state, activity),
                _ => throw new NotSupportedException($"The specified {typeof(ActionType).Name} '{action.Type}' is not supported"),
            };
        }

        protected virtual IWorkflowActivityProcessor CreateFunctionActivityProcessor(StateDefinition state, V1WorkflowActivity activity)
        {
            if (!state.TryGetAction(activity.Metadata, out var action))
                throw new NullReferenceException($"Failed to find an action that matches the metadata specified by the activity with id '{activity.Id}'");
            if (!this.Context.Workflow.Definition.TryGetFunction(action.Function!.RefName, out var function))
                throw new NullReferenceException($"Failed to find a function with the specified name '{action.Function.RefName}' in the workflow with name '{this.Context.Workflow.Definition.Id}' and version '{this.Context.Workflow.Definition.Version}'");
            return function.Type switch
            {
                FunctionType.AsyncApi => ActivatorUtilities.CreateInstance<AsyncApiFunctionProcessor>(this.ServiceProvider, activity, action, function),
                FunctionType.Expression => ActivatorUtilities.CreateInstance<RuntimeExpressionFunctionProcessor>(this.ServiceProvider, activity, action, function),
                FunctionType.GraphQL => ActivatorUtilities.CreateInstance<GraphQLFunctionProcessor>(this.ServiceProvider, activity, action, function),
                FunctionType.OData => ActivatorUtilities.CreateInstance<ODataFunctionProcessor>(this.ServiceProvider, activity, action, function),
                //FunctionType.OpenApi => //todo: move 'rest' code here, and implement basic rest type, for when the Serverless Workflow spec supports it
                FunctionType.Rest => ActivatorUtilities.CreateInstance<OpenApiFunctionProcessor>(this.ServiceProvider, activity, action, function),
                FunctionType.Rpc => ActivatorUtilities.CreateInstance<GrpcFunctionProcessor>(this.ServiceProvider, activity, action, function),
                _ => throw new NotSupportedException($"The specified {nameof(FunctionType)} '{function.Type}' is not supported"),
            };
        }

        protected virtual IWorkflowActivityProcessor CreateAsyncFunctionActivityProcessor(StateDefinition state, V1WorkflowActivity activity)
        {
            if (!state.TryGetAction(activity.Metadata, out var action))
                throw new NullReferenceException($"Failed to find an action that matches the metadata specified by the activity with id '{activity.Id}'");
            if (!this.Context.Workflow.Definition.TryGetEvent(action.Event!.ProduceEvent, out var triggerEvent))
                throw new NullReferenceException($"Failed to find a produced event with the specified name '{action.Event!.ProduceEvent}' in the workflow with name '{this.Context.Workflow.Definition.Id}' and version '{this.Context.Workflow.Definition.Version}'");
            if (triggerEvent.Kind != EventKind.Produced)
                throw new Exception($"The event '{action.Event!.ResultEvent}' is referenced as a produced event, but is defined as a consumed one");
            if (!this.Context.Workflow.Definition.TryGetEvent(action.Event!.ResultEvent, out var resultEvent))
                throw new NullReferenceException($"Failed to find a consumed event with the specified name '{action.Event!.ResultEvent}' in the workflow with name '{this.Context.Workflow.Definition.Id}' and version '{this.Context.Workflow.Definition.Version}'");
            if (resultEvent.Kind != EventKind.Consumed)
                throw new Exception($"The event '{action.Event!.ResultEvent}' is referenced as a consumed event, but is defined as a produced one");
            return ActivatorUtilities.CreateInstance<AsyncFunctionProcessor>(this.ServiceProvider, state, activity, action, triggerEvent, resultEvent);
        }

        protected virtual IWorkflowActivityProcessor CreateEventStateTriggerProcessor(StateDefinition state, V1WorkflowActivity activity)
        {
            if (state is not EventStateDefinition eventState)
                throw new ArgumentException($"The specified state definition with name '{state.Name}' is not the definition of an event state");
            if (!activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.Trigger, out var rawTriggerId))
                throw new ArgumentException($"The specified activity '{activity.Id}' is missing the required metadata field '{V1WorkflowActivityMetadata.Trigger}'");
            if (!int.TryParse(rawTriggerId, out var triggerId))
                throw new ArgumentException($"The '{V1WorkflowActivityMetadata.Trigger}' metadata field of activity '{activity.Id}' is not a valid integer");
            if (!eventState.TryGetTrigger(triggerId, out var trigger))
                throw new NullReferenceException($"Failed to find a trigger ath the specified index '{triggerId}' in the event state with name '{eventState.Name}'");
            return ActivatorUtilities.CreateInstance<EventStateTriggerProcessor>(this.ServiceProvider, activity, state, trigger);
        }

        protected virtual IWorkflowActivityProcessor CreateProduceEventActivityProcessor(V1WorkflowActivity activity)
        {
            if (!activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.Event, out var eventName))
                throw new NullReferenceException($"Failed to find the required '{V1WorkflowActivityMetadata.Event}' metadata");
            if (!this.Context.Workflow.Definition.TryGetEvent(eventName, out var e))
                throw new NullReferenceException($"Failed to find an action that matches the metadata specified by the activity with id '{activity.Id}'");
            return ActivatorUtilities.CreateInstance<ProduceEventProcessor>(this.ServiceProvider, activity, e);
        }

        protected virtual IWorkflowActivityProcessor CreateConsumeEventActivityProcessor(V1WorkflowActivity activity)
        {
            if (!activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.Event, out var eventName))
                throw new NullReferenceException($"Failed to find the required '{V1WorkflowActivityMetadata.Event}' metadata");
            if (!this.Context.Workflow.Definition.TryGetEvent(eventName, out var e))
                throw new NullReferenceException($"Failed to find an action that matches the metadata specified by the activity with id '{activity.Id}'");
            return ActivatorUtilities.CreateInstance<ConsumeEventProcessor>(this.ServiceProvider, activity, e);
        }

        IWorkflowActivityProcessor IWorkflowActivityProcessorFactory.Create(V1WorkflowActivity activity)
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

        TProcessor IWorkflowActivityProcessorFactory.Create<TProcessor>(V1WorkflowActivity activity)
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
