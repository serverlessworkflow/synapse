/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{

    /// <summary>
    /// Defines extensions for <see cref="StateDefinition"/>s
    /// </summary>
    public static class StateDefinitionExtensions
    {

        /// <summary>
        /// Gets the <see cref="StateDefinition"/>'s <see cref="WorkflowOutcome"/>
        /// </summary>
        /// <param name="state">The <see cref="StateDefinition"/> to get the <see cref="WorkflowOutcome"/> of</param>
        /// <returns>The <see cref="StateDefinition"/>'s <see cref="WorkflowOutcome"/></returns>
        public static WorkflowOutcome GetOutcome(this StateDefinition state)
        {
            if (state.IsEnd || state.End != null)
                return new(WorkflowOutcomeType.End, state.End);
            else
                return new(WorkflowOutcomeType.Transition, string.IsNullOrWhiteSpace(state.TransitionToStateName)? state.Transition : state.TransitionToStateName);
        }

        /// <summary>
        /// Sets the <see cref="StateDefinition"/>'s <see cref="WorkflowOutcome"/>
        /// </summary>
        /// <param name="state">The <see cref="StateDefinition"/> to set the <see cref="WorkflowOutcome"/> of</param>
        /// <param name="outcome">The <see cref="WorkflowOutcome"/> to set</param>
        public static void SetOutcome(this StateDefinition state, WorkflowOutcome outcome)
        {
            if (outcome == null) throw new ArgumentNullException(nameof(outcome));
            switch (outcome.Type)
            {
                case WorkflowOutcomeType.Transition:
                    state.IsEnd = false;
                    state.End = null;
                    if(outcome.Definition != null)
                    {
                        switch (outcome.Definition)
                        {
                            case string transitionToStateName:
                                state.TransitionToStateName = transitionToStateName;
                                break;
                            case TransitionDefinition transition:
                                state.Transition = transition;
                                break;
                        }
                    }
                    break;
                case WorkflowOutcomeType.End:
                    state.TransitionToStateName = null;
                    state.Transition = null;
                    if (outcome.Definition == null)
                    {
                        state.IsEnd = true;
                    }
                    else
                    {
                        switch (outcome.Definition)
                        {
                            case EndDefinition end:
                                state.End = end;
                                break;
                            default:
                                state.End = null;
                                state.IsEnd = true;
                                break;
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException($"The specified {nameof(WorkflowOutcomeType)} '{outcome.Type}' is not supported");
            }
        }

        /// <summary>
        /// Converts the <see cref="StateDefinition"/> to a new <see cref="StateDefinition"/> of the specified type
        /// </summary>
        /// <param name="state">The <see cref="StateDefinition"/> to convert</param>
        /// <param name="type">The type to convert the <see cref="StateDefinition"/> to</param>
        /// <returns>A new <see cref="StateDefinition"/> of the specified type</returns>
        public static StateDefinition OfType(this StateDefinition state, StateType type)
        {
           var result = type switch
            {
                StateType.Callback => (StateDefinition)new CallbackStateDefinition(),
                StateType.Event => new EventStateDefinition(),
                StateType.ForEach => new ForEachStateDefinition(),
                StateType.Inject => new InjectStateDefinition(),
                StateType.Operation => new OperationStateDefinition(),
                StateType.Parallel => new ParallelStateDefinition(),
                StateType.Sleep => new SleepStateDefinition(),
                StateType.Switch => new SwitchStateDefinition(),
                _ => throw new NotSupportedException($"The specified {nameof(StateType)} '{type}' is not supported")
            };
            result.CompensatedBy = state.CompensatedBy;
            result.DataFilter = state.DataFilter;
            result.DataInputSchema = state.DataInputSchema;
            if (state.IsEnd)
                result.IsEnd = true;
            else if(state.End != null)
                result.End = state.End;
            result.Errors = state.Errors;
            result.Id = state.Id;
            result.Metadata = state.Metadata;
            result.Name = state.Name;
            if(string.IsNullOrEmpty(state.TransitionToStateName))
                result.Transition = state.Transition;
            else
                result.TransitionToStateName = state.TransitionToStateName;
            result.UsedForCompensation = state.UsedForCompensation;
            if(result is SwitchStateDefinition)
            {
                result.Transition = null;
                result.TransitionToStateName = null;
                result.IsEnd = false;
                result.End = null;
            }
            return result;
        }

    }

}
