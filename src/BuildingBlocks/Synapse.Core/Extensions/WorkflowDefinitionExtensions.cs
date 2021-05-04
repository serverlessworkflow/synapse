using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;
using System;
using System.Linq;

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="WorkflowDefinition"/>s
    /// </summary>
    public static class WorkflowDefinitionExtensions
    {

        /// <summary>
        /// Gets the startup <see cref="StateDefinition"/>
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified <see cref="StateDefinition"/></param>
        /// <returns>The startup <see cref="StateDefinition"/></returns>
        public static StateDefinition GetStartupState(this WorkflowDefinition workflowDefinition)
        {
            return workflowDefinition.States.SingleOrDefault(s => s.Name == workflowDefinition.Start.StateName);
        }

        /// <summary>
        /// Gets the startup <see cref="StateDefinition"/>
        /// </summary>
        /// <typeparam name="TState">The expected type of the <see cref="WorkflowDefinition"/>'s startup <see cref="StateDefinition"/></typeparam>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified <see cref="StateDefinition"/></param>
        /// <returns>The startup <see cref="StateDefinition"/></returns>
        public static TState GetStartupState<TState>(this WorkflowDefinition workflowDefinition)
            where TState : StateDefinition
        {
            return workflowDefinition.States.SingleOrDefault(s => s.Name == workflowDefinition.Start.StateName) as TState;
        }

        /// <summary>
        /// Attempts to the startup <see cref="StateDefinition"/>
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified <see cref="StateDefinition"/></param>
        /// <param name="state">The startup <see cref="StateDefinition"/></param>
        /// <returns>A boolean indicating whether or not the <see cref="WorkflowDefinition"/>'s startup <see cref="StateDefinition"/> could be found</returns>
        public static bool TryGetStartupState(this WorkflowDefinition workflowDefinition, out StateDefinition state)
        {
            state = workflowDefinition.GetStartupState();
            return state != null;
        }

        /// <summary>
        /// Gets the <see cref="StateDefinition"/> with the specified name
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified <see cref="StateDefinition"/></param>
        /// <param name="name">The name of the <see cref="StateDefinition"/> to get</param>
        /// <returns>The <see cref="StateDefinition"/> with the specified id</returns>
        public static StateDefinition GetState(this WorkflowDefinition workflowDefinition, string name)
        {
            return workflowDefinition.States.FirstOrDefault(s => s.Name == name);
        }

        /// <summary>
        /// Attempts to get the <see cref="StateDefinition"/> with the specified name
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified <see cref="StateDefinition"/></param>
        /// <param name="name">The name of the <see cref="StateDefinition"/> to get</param>
        /// <param name="state">The <see cref="StateDefinition"/> with the specified id</param>
        /// <returns>A boolean indicating whether or not a <see cref="StateDefinition"/> with the specified id could be found</returns>
        public static bool TryGetState(this WorkflowDefinition workflowDefinition, string name, out StateDefinition state)
        {
            state = workflowDefinition.GetState(name);
            return state != null;
        }

        /// <summary>
        /// Attempts to get the <see cref="StateDefinition"/> with the specified name
        /// </summary>
        /// <typeparam name="TState">The type of <see cref="StateDefinition"/> to get</typeparam>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified <see cref="StateDefinition"/></param>
        /// <param name="name">The name of the <see cref="StateDefinition"/> to get</param>
        /// <param name="state">The <see cref="StateDefinition"/> with the specified id</param>
        /// <returns>A boolean indicating whether or not a <see cref="StateDefinition"/> with the specified id could be found</returns>
        public static bool TryGetState<TState>(this WorkflowDefinition workflowDefinition, string name, out TState state)
            where TState : StateDefinition
        {
            state = workflowDefinition.GetState(name) as TState;
            return state != null;
        }

        /// <summary>
        /// Attempts to get the <see cref="StateDefinition"/> that comes after the one with the specified name
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search</param>
        /// <param name="previousStateName">The name of the <see cref="StateDefinition"/> to find the next <see cref="StateDefinition"/> for</param>
        /// <param name="state">The next <see cref="StateDefinition"/></param>
        /// <returns>A boolean indicating whether or not the next <see cref="StateDefinition"/> could be found</returns>
        public static bool TryGetNextState(this WorkflowDefinition workflowDefinition, string previousStateName, out StateDefinition state)
        {
            state = null;
            if (!workflowDefinition.TryGetState(previousStateName, out StateDefinition previousState)
                || previousState.End != null
                || previousState.Transition == null)
                return false;
            return workflowDefinition.TryGetState(previousState.Transition.To, out state);
        }

        /// <summary>
        /// Gets the <see cref="FunctionDefinition"/> with the specified name
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified <see cref="FunctionDefinition"/></param>
        /// <param name="name">The name of the <see cref="FunctionDefinition"/> to get</param>
        /// <returns>The <see cref="FunctionDefinition"/> with the specified name</returns>
        public static FunctionDefinition GetFunction(this WorkflowDefinition workflowDefinition, string name)
        {
            return workflowDefinition.Functions.FirstOrDefault(s => s.Name == name);
        }

        /// <summary>
        /// Attempts to get the <see cref="FunctionDefinition"/> with the specified name
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified <see cref="FunctionDefinition"/></param>
        /// <param name="name">The name of the <see cref="FunctionDefinition"/> to get</param>
        /// <param name="function">The <see cref="FunctionDefinition"/> with the specified name</param>
        /// <returns>A boolean indicating whether or not a <see cref="FunctionDefinition"/> with the specified name could be found</returns>
        public static bool TryGetFunction(this WorkflowDefinition workflowDefinition, string name, out FunctionDefinition function)
        {
            function = workflowDefinition.GetFunction(name);
            return function != null;
        }

        /// <summary>
        /// Gets the <see cref="EventDefinition"/> with the specified name
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified <see cref="EventDefinition"/></param>
        /// <param name="name">The name of the <see cref="EventDefinition"/> to get</param>
        /// <returns>The <see cref="EventDefinition"/> with the specified name</returns>
        public static EventDefinition GetEvent(this WorkflowDefinition workflowDefinition, string name)
        {
            return workflowDefinition.Events.FirstOrDefault(s => s.Name == name);
        }

        /// <summary>
        /// Attempts to get the <see cref="EventDefinition"/> with the specified name
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified <see cref="EventDefinition"/></param>
        /// <param name="name">The name of the <see cref="EventDefinition"/> to get</param>
        /// <param name="e">The <see cref="EventDefinition"/> with the specified name</param>
        /// <returns>A boolean indicating whether or not a <see cref="EventDefinition"/> with the specified name could be found</returns>
        public static bool TryGetEvent(this WorkflowDefinition workflowDefinition, string name, out EventDefinition e)
        {
            e = workflowDefinition.GetEvent(name);
            return e != null;
        }

        /// <summary>
        /// Gets the <see cref="RetryStrategyDefinition"/> with the specified name
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified <see cref="RetryStrategyDefinition"/></param>
        /// <param name="name">The name of the <see cref="RetryStrategyDefinition"/> to get</param>
        /// <returns>The <see cref="RetryStrategyDefinition"/> with the specified name</returns>
        public static RetryStrategyDefinition GetRetryStrategy(this WorkflowDefinition workflowDefinition, string name)
        {
            return workflowDefinition.Retries.FirstOrDefault(s => s.Name == name);
        }

        /// <summary>
        /// Attempts to get the <see cref="RetryStrategyDefinition"/> with the specified name
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified <see cref="RetryStrategyDefinition"/></param>
        /// <param name="name">The name of the <see cref="RetryStrategyDefinition"/> to get</param>
        /// <param name="strategy">The <see cref="RetryStrategyDefinition"/> with the specified name</param>
        /// <returns>A boolean indicating whether or not a <see cref="RetryStrategyDefinition"/> with the specified name could be found</returns>
        public static bool TryGetRetryStrategy(this WorkflowDefinition workflowDefinition, string name, out RetryStrategyDefinition strategy)
        {
            strategy = workflowDefinition.GetRetryStrategy(name);
            return strategy != null;
        }

        /// <summary>
        /// Gets the compensation <see cref="StateDefinition"/> with the specified id
        /// </summary>
        /// <param name="workflowDefinition">The <see cref="WorkflowDefinition"/> to search for the specified compensation <see cref="StateDefinition"/></param>
        /// <param name="compensationStateId">The id of the compensation <see cref="StateDefinition"/> to get</param>
        /// <returns>The compensation <see cref="StateDefinition"/> with the specified id</returns>
        public static StateDefinition GetCompensationState(this WorkflowDefinition workflowDefinition, string compensationStateId)
        {
            StateDefinition compensationStateDefinition = workflowDefinition.States.FirstOrDefault(s => s.Id == compensationStateId);
            if (compensationStateDefinition == null)
                throw new NullReferenceException($"Failed to find a state with id '{compensationStateId}' in workflow definition with id '{workflowDefinition.Id}'");
            if (!compensationStateDefinition.UseForCompensation)
                throw new NotSupportedException($"The {nameof(StateDefinition.UseForCompensation)} property of the state with id '{compensationStateId}' in workflow definition with id '{workflowDefinition.Id}' must be set to 'True' to be used for compensation");
            if (compensationStateDefinition.Type == StateType.Event)
                throw new NotSupportedException($"The state with id '{compensationStateId}' cannot be used for compensation because it is of type '{StateType.Event}'");
            return compensationStateDefinition;
        }

    }

}
