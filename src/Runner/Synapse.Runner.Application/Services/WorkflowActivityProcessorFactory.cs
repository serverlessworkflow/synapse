using Microsoft.Extensions.DependencyInjection;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;
using Synapse.Runner.Application.Services.Processors;
using System;

namespace Synapse.Runner.Application.Services
{

    /// <summary>
    /// Represents the default implementation of the <see cref="IWorkflowActivityProcessorFactory"/>
    /// </summary>
    public class WorkflowActivityProcessorFactory
        : IWorkflowActivityProcessorFactory
    {

        /// <summary>
        /// Initializes a new <see cref="WorkflowActivityProcessorFactory"/>
        /// </summary>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
        /// <param name="executionContext">The current <see cref="IWorkflowExecutionContext"/></param>
        public WorkflowActivityProcessorFactory(IServiceProvider serviceProvider, IWorkflowExecutionContext executionContext)
        {
            this.ServiceProvider = serviceProvider;
            this.ExecutionContext = executionContext;
        }

        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the current <see cref="IWorkflowExecutionContext"/>
        /// </summary>
        protected IWorkflowExecutionContext ExecutionContext { get; }

        /// <summary>
        /// Creates a new <see cref="IWorkflowActivityProcessor"/> for the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="activity">The <see cref="V1WorkflowActivity"/> to create a new <see cref="IWorkflowActivityProcessor"/> for</param>
        /// <returns>A new <see cref="IWorkflowActivityProcessor"/></returns>
        protected virtual IWorkflowActivityProcessor Create(V1WorkflowActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            StateDefinition state = null;
            if (!this.ExecutionContext.Definition.Spec.Definition.TryGetState(activity.Metadata.State, out state))
                throw new NullReferenceException($"Failed to find the workflow state with the specified name '{activity.Metadata.State}'");
            switch (activity.Type)
            {
                case V1WorkflowActivityType.State:
                    switch (state)
                    {
                        //case CallbackStateDefinition callbackState:
                        //    return ActivatorUtilities.CreateInstance<CallbackStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        //case DelayStateDefinition delayState:
                        //    return ActivatorUtilities.CreateInstance<DelayStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        //case EventStateDefinition eventState:
                        //    return ActivatorUtilities.CreateInstance<EventStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        //case ForEachStateDefinition forEachState:
                        //    return ActivatorUtilities.CreateInstance<ForEachStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        case InjectStateDefinition injectState:
                            return ActivatorUtilities.CreateInstance<InjectStateProcessor>(this.ServiceProvider, state, activity);
                        case OperationStateDefinition operationState:
                            return ActivatorUtilities.CreateInstance<OperationStateProcessor>(this.ServiceProvider, state, activity);
                        //case ParallelStateDefinition parallelState:
                        //    return ActivatorUtilities.CreateInstance<ParallelStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        //case SubFlowStateDefinition subFlowState:
                        //    return ActivatorUtilities.CreateInstance<SubFlowStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        //case SwitchStateDefinition switchState:
                        //    return ActivatorUtilities.CreateInstance<SwitchStateProcessor>(this.ServiceProvider, expressionEvaluator, state, activity);
                        default:
                            throw new NotSupportedException($"The specified {nameof(StateDefinition)} type '{state.GetType().Name}' is not supported");
                    }
                case V1WorkflowActivityType.Action:
                    ActionDefinition action;
                    switch (state)
                    {
                        case OperationStateDefinition operationState:
                            if (!operationState.TryGetAction(activity.Metadata.Action, out action))
                                throw new NullReferenceException($"Failed to find an action with the specified name '{action.Name}' in the state with name '{state.Name}'");
                            break;
                        //case ForEachStateDefinition forEachState:
                        //    if (!forEachState.TryGetAction(actionPointer.ActionName, out action))
                        //        throw new NullReferenceException($"Failed to find an action with the specified name '{action.Name}' in the state with name '{state.Name}'");
                        //    break;
                        //case CallbackStateDefinition callbackState:
                        //    action = callbackState.Action;
                        //    break;
                        //case ParallelStateDefinition parallelState:
                        //    if (!parallelState.TryGetBranch(actionPointer.BranchName, out branch))
                        //        throw new NullReferenceException($"Failed to find a branch with the specified name '{actionPointer.BranchName}' in the state with name '{state.Name}'");
                        //    if (!branch.TryGetAction(actionPointer.ActionName, out action))
                        //        throw new NullReferenceException($"Failed to find an action with the specified name '{action.Name}' in the branch with name '{branch.Name}'");
                        //    break;
                        //case EventStateDefinition eventState:
                        //    if (!eventState.TryGetTrigger(actionPointer.TriggerId.Value, out trigger))
                        //        throw new NullReferenceException($"Failed to find a branch with the specified name '{actionPointer.BranchName}' in the state with name '{state.Name}'");
                        //    if (!trigger.TryGetAction(actionPointer.ActionName, out action))
                        //        throw new NullReferenceException($"Failed to find an action with the specified name '{action.Name}' in the trigger with id '{actionPointer.TriggerId}'");
                        //    break;
                        default:
                            throw new NotSupportedException($"The specified {nameof(StateDefinition)} type '{state.GetType().Name}' is not supported in this context");
                    }
                    switch (action.Type)
                    {
                        case ActionType.FunctionCall:
                            if (!this.ExecutionContext.Definition.Spec.Definition.TryGetFunction(action.Function.Name, out FunctionDefinition function))
                                throw new NullReferenceException($"Failed to find a function with the specified name '{action.Function.Name}' in the workflow with name '{this.ExecutionContext.Definition.Spec.Definition.Id}' and version '{this.ExecutionContext.Definition.Spec.Definition.Version}'");
                            return ActivatorUtilities.CreateInstance<FunctionProcessor>(this.ServiceProvider, activity, action, function);
                        //case ActionType.EventTrigger:
                        //    if (!workflowDefinition.TryGetEvent(action.Event.TriggerEvent, out EventDefinition triggerEvent))
                        //        throw new NullReferenceException($"Failed to find an event with the specified name '{action.Event.TriggerEvent}' in the workflow with name '{workflowInstance.DefinitionId}' and version '{workflowInstance.DefinitionVersion}'");
                        //    if (!workflowDefinition.TryGetEvent(action.Event.ResultEvent, out EventDefinition resultEvent))
                        //        throw new NullReferenceException($"Failed to find an event with the specified name '{action.Event.ResultEvent}' in the workflow with name '{workflowInstance.DefinitionId}' and version '{workflowInstance.DefinitionVersion}'");
                        //    return ActivatorUtilities.CreateInstance<EventActionProcessor>(this.ServiceProvider, expressionEvaluator, workflowInstance, state, pointer, action, triggerEvent, resultEvent);
                        default:
                            throw new NotSupportedException($"The specified {nameof(ActionType)} '{action.Type}' is not supported");
                    }
                case V1WorkflowActivityType.End:
                    return ActivatorUtilities.CreateInstance<EndProcessor>(this.ServiceProvider, state.End, activity);
                default:
                    throw new NotSupportedException($"The specified {nameof(V1WorkflowActivity)} type '{activity.Type}' is not supported");
            }
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
