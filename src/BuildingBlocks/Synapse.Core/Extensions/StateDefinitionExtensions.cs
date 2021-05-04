using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;
using System;
using System.Linq;

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="StateDefinition"/>s
    /// </summary>
    public static class StateDefinitionExtensions
    {

        /// <summary>
        /// Gets the <see cref="ErrorHandlerDefinition"/> used to handle the specified error
        /// </summary>
        /// <param name="state">The <see cref="StateDefinition"/> to search</param>
        /// <param name="error">The error to handle</param>
        /// <param name="code">The code of the error to handle</param>
        /// <param name="errorHandler">The <see cref="ErrorHandlerDefinition"/> used to handle the specified error</param>
        /// <returns>The <see cref="ErrorHandlerDefinition"/> used to handle the specified error</returns>
        public static bool TryGetErrorHandlerFor(this StateDefinition state, string error, string code, out ErrorHandlerDefinition errorHandler)
        {
            errorHandler = state.Errors?
                .FirstOrDefault(h => 
                    h.Error.Equals(error, StringComparison.InvariantCultureIgnoreCase) 
                    && string.IsNullOrWhiteSpace(h.Code) ?  
                        string.IsNullOrWhiteSpace(code) : h.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase));
            if(errorHandler == null)
                errorHandler = state.Errors?
                    .FirstOrDefault(h => h.Error == "*");
            return (errorHandler != null);
        }

        /// <summary>
        /// Gets the <see cref="ActionDefinition"/> with the specified name
        /// </summary>
        /// <param name="state">The <see cref="OperationStateDefinition"/> to search for the specified <see cref="ActionDefinition"/></param>
        /// <param name="name">The name of the <see cref="ActionDefinition"/> to get</param>
        /// <returns>The <see cref="ActionDefinition"/> with the specified name</returns>
        public static ActionDefinition GetAction(this OperationStateDefinition state, string name)
        {
            return state.Actions.FirstOrDefault(s => s.Name == name);
        }

        /// <summary>
        /// Attempts to get the <see cref="ActionDefinition"/> with the specified name
        /// </summary>
        /// <param name="state">The <see cref="OperationStateDefinition"/> to search for the specified <see cref="ActionDefinition"/></param>
        /// <param name="name">The name of the <see cref="ActionDefinition"/> to get</param>
        /// <param name="action">The <see cref="ActionDefinition"/> with the specified name</param>
        /// <returns>A boolean indicating whether or not a <see cref="ActionDefinition"/> with the specified name could be found</returns>
        public static bool TryGetAction(this OperationStateDefinition state, string name, out ActionDefinition action)
        {
            action = state.GetAction(name);
            return action != null;
        }

        /// <summary>
        /// Attempts to get the next <see cref="ActionDefinition"/> in the pipeline
        /// </summary>
        /// <param name="state">The <see cref="OperationStateDefinition"/> to search</param>
        /// <param name="previousActionName">The name of the <see cref="ActionDefinition"/> to get the next <see cref="ActionDefinition"/> for</param>
        /// <param name="action">The next <see cref="ActionDefinition"/>, if any</param>
        /// <returns>A boolean indicating whether or not there is a next <see cref="ActionDefinition"/> in the pipeline</returns>
        public static bool TryGetNextAction(this OperationStateDefinition state, string previousActionName, out ActionDefinition action)
        {
            action = null;
            ActionDefinition previousAction = state.Actions.FirstOrDefault(a => a.Name == previousActionName);
            int previousActionIndex = state.Actions.ToList().IndexOf(previousAction);
            int nextIndex = previousActionIndex + 1;
            if (nextIndex >= state.Actions.Count())
                return false;
            action = state.Actions.ElementAt(nextIndex);
            return true;
        }

        /// <summary>
        /// Gets the <see cref="ActionDefinition"/> with the specified name
        /// </summary>
        /// <param name="state">The <see cref="ForEachStateDefinition"/> to search for the specified <see cref="ActionDefinition"/></param>
        /// <param name="name">The name of the <see cref="ActionDefinition"/> to get</param>
        /// <returns>The <see cref="ActionDefinition"/> with the specified name</returns>
        public static ActionDefinition GetAction(this ForEachStateDefinition state, string name)
        {
            return state.Actions.FirstOrDefault(s => s.Name == name);
        }

        /// <summary>
        /// Attempts to get the <see cref="ActionDefinition"/> with the specified name
        /// </summary>
        /// <param name="state">The <see cref="ForEachStateDefinition"/> to search for the specified <see cref="ActionDefinition"/></param>
        /// <param name="name">The name of the <see cref="ActionDefinition"/> to get</param>
        /// <param name="action">The <see cref="ActionDefinition"/> with the specified name</param>
        /// <returns>A boolean indicating whether or not a <see cref="ActionDefinition"/> with the specified name could be found</returns>
        public static bool TryGetAction(this ForEachStateDefinition state, string name, out ActionDefinition action)
        {
            action = state.GetAction(name);
            return action != null;
        }

        /// <summary>
        /// Attempts to get the next <see cref="ActionDefinition"/> in the pipeline
        /// </summary>
        /// <param name="state">The <see cref="ForEachStateDefinition"/> to search</param>
        /// <param name="previousActionName">The name of the <see cref="ActionDefinition"/> to get the next <see cref="ActionDefinition"/> for</param>
        /// <param name="action">The next <see cref="ActionDefinition"/>, if any</param>
        /// <returns>A boolean indicating whether or not there is a next <see cref="ActionDefinition"/> in the pipeline</returns>
        public static bool TryGetNextAction(this ForEachStateDefinition state, string previousActionName, out ActionDefinition action)
        {
            action = null;
            ActionDefinition previousAction = state.Actions.FirstOrDefault(a => a.Name == previousActionName);
            int previousActionIndex = state.Actions.ToList().IndexOf(previousAction);
            int nextIndex = previousActionIndex + 1;
            if (nextIndex >= state.Actions.Count())
                return false;
            action = state.Actions.ElementAt(nextIndex);
            return true;
        }

        /// <summary>
        /// Gets the <see cref="BranchDefinition"/> with the specified name
        /// </summary>
        /// <param name="state">The <see cref="ParallelStateDefinition"/> to search for the specified <see cref="BranchDefinition"/></param>
        /// <param name="name">The name of the <see cref="BranchDefinition"/> to get</param>
        /// <returns>The <see cref="BranchDefinition"/> with the specified name</returns>
        public static BranchDefinition GetBranch(this ParallelStateDefinition state, string name)
        {
            return state.Branches.FirstOrDefault(b => b.Name == name);
        }

        /// <summary>
        /// Attempts to get the <see cref="BranchDefinition"/> with the specified name
        /// </summary>
        /// <param name="state">The <see cref="ParallelStateDefinition"/> to search for the specified <see cref="BranchDefinition"/></param>
        /// <param name="name">The name of the <see cref="BranchDefinition"/> to get</param>
        /// <param name="branch">The <see cref="BranchDefinition"/> with the specified name</param>
        /// <returns>A boolean indicating whether or not a <see cref="BranchDefinition"/> with the specified name could be found</returns>
        public static bool TryGetBranch(this ParallelStateDefinition state, string name, out BranchDefinition branch)
        {
            branch = state.GetBranch(name);
            return branch != null;
        }

        /// <summary>
        /// Gets the <see cref="EventStateTriggerDefinition"/> with the specified id
        /// </summary>
        /// <param name="state">The <see cref="EventStateDefinition"/> to search for the specified <see cref="EventStateTriggerDefinition"/></param>
        /// <param name="id">The id of the <see cref="EventStateTriggerDefinition"/> to get</param>
        /// <returns>The <see cref="EventStateTriggerDefinition"/> with the specified id</returns>
        public static EventStateTriggerDefinition GetTrigger(this EventStateDefinition state, int id)
        {
            return state.Triggers.ElementAt(id);
        }

        /// <summary>
        /// Attempts to get the <see cref="EventStateTriggerDefinition"/> with the specified id
        /// </summary>
        /// <param name="state">The <see cref="EventStateDefinition"/> to search for the specified <see cref="EventStateTriggerDefinition"/></param>
        /// <param name="id">The name of the <see cref="EventStateTriggerDefinition"/> to get</param>
        /// <param name="trigger">The <see cref="EventStateTriggerDefinition"/> with the specified id</param>
        /// <returns>A boolean indicating whether or not a <see cref="EventStateTriggerDefinition"/> with the specified id could be found</returns>
        public static bool TryGetTrigger(this EventStateDefinition state, int id, out EventStateTriggerDefinition trigger)
        {
            trigger = null;
            try
            {
                trigger = state.GetTrigger(id);
            }
            catch
            {
                return false;
            }
            return trigger != null;
        }

        /// <summary>
        /// Gets the <see cref="SwitchCaseDefinition"/> with the specified name
        /// </summary>
        /// <param name="state">The <see cref="SwitchStateDefinition"/> to search for the <see cref="SwitchCaseDefinition"/> with the specified name</param>
        /// <param name="conditionName">The name of the <see cref="SwitchCaseDefinition"/> to get</param>
        /// <returns>The <see cref="SwitchCaseDefinition"/> with the specified name</returns>
        public static SwitchCaseDefinition GetCondition(this SwitchStateDefinition state, string conditionName)
        {
            SwitchCaseDefinition @case;
            switch (state.SwitchType)
            {
                case SwitchStateType.Data:
                    if (conditionName == "default")
                        @case = new DataCaseDefinition() { Name = "default", Transition = state.Default.Transition, End = state.Default.End };
                    else
                        @case = state.DataConditions.Single(c => c.Name == conditionName);
                    break;
                case SwitchStateType.Event:
                    if (conditionName == "default")
                        @case = new EventCaseDefinition() { Name = "default", Transition = state.Default.Transition, End = state.Default.End };
                    else
                        @case = state.EventConditions.Single(c => c.Name == conditionName);
                    break;
                default:
                    throw new NotSupportedException($"The specified switch state type '{state.SwitchType}' is not supported in this context");
            }
            return @case;
        }

        /// <summary>
        /// Attempts to get the <see cref="SwitchCaseDefinition"/> with the specified name
        /// </summary>
        /// <param name="state">The <see cref="SwitchStateDefinition"/> to search for the <see cref="SwitchCaseDefinition"/> with the specified name</param>
        /// <param name="conditionName">The name of the <see cref="SwitchCaseDefinition"/> to get</param>
        /// <param name="condition">The <see cref="SwitchCaseDefinition"/> with the specified name</param>
        /// <returns>A boolean indicating whether or not the <see cref="SwitchCaseDefinition"/> with the specified name could be found</returns>
        public static bool TryGetCondition(this SwitchStateDefinition state, string conditionName, out SwitchCaseDefinition condition)
        {
            condition = null;
            try
            {
                condition = state.GetCondition(conditionName);
            }
            catch
            {
                return false;
            }
            return condition != null;
        }

        /// <summary>
        /// Gets the <see cref="EventCaseDefinition"/> that applies to the specified event
        /// </summary>
        /// <param name="state">The <see cref="SwitchStateDefinition"/> to search for the <see cref="EventCaseDefinition"/> that applies to the specified event</param>
        /// <param name="eventReference">The name of the event the <see cref="EventCaseDefinition"/> to get applies to</param>
        /// <returns>The <see cref="EventCaseDefinition"/> that applies to the specified event</returns>
        public static EventCaseDefinition GetEventCondition(this SwitchStateDefinition state, string eventReference)
        {
            return state.EventConditions?.FirstOrDefault(c => c.Event == eventReference);
        }

        /// <summary>
        /// Attempts to get the <see cref="EventCaseDefinition"/> that applies to the specified event
        /// </summary>
        /// <param name="state">The <see cref="SwitchStateDefinition"/> to search for the specified <see cref="EventCaseDefinition"/></param>
        /// <param name="eventReference">The reference of the event the <see cref="EventCaseDefinition"/> to get applies to</param>
        /// <param name="condition">The <see cref="EventCaseDefinition"/> that applies to the specified event</param>
        /// <returns>A boolean indicating whether or not a <see cref="EventCaseDefinition"/> with the specified id could be found</returns>
        public static bool TryGetEventCondition(this SwitchStateDefinition state, string eventReference, out EventCaseDefinition condition)
        {
            condition = null;
            try
            {
                condition = state.GetEventCondition(eventReference);
            }
            catch
            {
                return false;
            }
             return condition != null;
        }

    }

}
