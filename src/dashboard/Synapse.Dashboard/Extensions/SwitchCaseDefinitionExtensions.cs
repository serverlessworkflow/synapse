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
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Defines extensions for <see cref="SwitchCaseDefinition"/>s
    /// </summary>
    public static class SwitchCaseDefinitionExtensions
    {

        /// <summary>
        /// Gets the <see cref="SwitchCaseDefinition"/>'s <see cref="WorkflowOutcome"/>
        /// </summary>
        /// <param name="switchCase">The <see cref="SwitchCaseDefinition"/> to get the <see cref="WorkflowOutcome"/> of</param>
        /// <returns>The <see cref="SwitchCaseDefinition"/>'s <see cref="WorkflowOutcome"/></returns>
        public static WorkflowOutcome GetOutcome(this SwitchCaseDefinition switchCase)
        {
            if (switchCase.IsEnd || switchCase.End != null)
                return new(WorkflowOutcomeType.End, switchCase.End);
            else
                return new(WorkflowOutcomeType.Transition, string.IsNullOrWhiteSpace(switchCase.TransitionToStateName) ? switchCase.Transition : switchCase.TransitionToStateName);
        }

        /// <summary>
        /// Sets the <see cref="SwitchCaseDefinition"/>'s <see cref="WorkflowOutcome"/>
        /// </summary>
        /// <param name="switchCase">The <see cref="SwitchCaseDefinition"/> to set the <see cref="WorkflowOutcome"/> of</param>
        /// <param name="outcome">The <see cref="WorkflowOutcome"/> to set</param>
        public static void SetOutcome(this SwitchCaseDefinition switchCase, WorkflowOutcome outcome)
        {
            if (outcome == null) throw new ArgumentNullException(nameof(outcome));
            switch (outcome.Type)
            {
                case WorkflowOutcomeType.Transition:
                    switchCase.IsEnd = false;
                    switchCase.End = null;
                    if (outcome.Definition != null)
                    {
                        switch (outcome.Definition)
                        {
                            case string transitionToStateName:
                                switchCase.TransitionToStateName = transitionToStateName;
                                break;
                            case TransitionDefinition transition:
                                switchCase.Transition = transition;
                                break;
                        }
                    }
                    break;
                case WorkflowOutcomeType.End:
                    switchCase.TransitionToStateName = null;
                    switchCase.Transition = null;
                    if (outcome.Definition == null)
                    {
                        switchCase.IsEnd = true;
                    }
                    else
                    {
                        switch (outcome.Definition)
                        {
                            case EndDefinition end:
                                switchCase.End = end;
                                break;
                            default:
                                switchCase.End = null;
                                switchCase.IsEnd = true;
                                break;
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException($"The specified {nameof(WorkflowOutcomeType)} '{outcome.Type}' is not supported");
            }
        }

    }

}
