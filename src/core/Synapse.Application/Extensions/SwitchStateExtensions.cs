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

using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="SwitchStateDefinition"/>s
    /// </summary>
    public static class SwitchStateExtensions
    {

        /// <summary>
        /// Gets the <see cref="SwitchCaseDefinition"/> with the specified name
        /// </summary>
        /// <param name="state">The <see cref="SwitchStateDefinition"/> to search for the <see cref="SwitchCaseDefinition"/> with the specified name</param>
        /// <param name="caseName">The name of the <see cref="SwitchCaseDefinition"/> to get</param>
        /// <returns>The <see cref="SwitchCaseDefinition"/> with the specified name</returns>
        public static SwitchCaseDefinition? GetCase(this SwitchStateDefinition state, string caseName)
        {
            SwitchCaseDefinition @case;
            switch (state.SwitchType)
            {
                case SwitchStateType.Data:
                    if (caseName == "default")
                        @case = new DataCaseDefinition() { Name = "default", Transition = state.DefaultCondition.Transition, End = state.DefaultCondition.End };
                    else
                        @case = state.DataConditions?.Single(c => c.Name == caseName)!;
                    break;
                case SwitchStateType.Event:
                    if (caseName == "default")
                        @case = new EventCaseDefinition() { Name = "default", Transition = state.DefaultCondition.Transition, End = state.DefaultCondition.End };
                    else
                        @case = state.EventConditions?.Single(c => c.Name == caseName)!;
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
        /// <param name="caseName">The name of the <see cref="SwitchCaseDefinition"/> to get</param>
        /// <param name="case">The <see cref="SwitchCaseDefinition"/> with the specified name</param>
        /// <returns>A boolean indicating whether or not the <see cref="SwitchCaseDefinition"/> with the specified name could be found</returns>
        public static bool TryGetCase(this SwitchStateDefinition state, string caseName, out SwitchCaseDefinition @case)
        {
            @case = null!;
            try
            {
                @case = state.GetCase(caseName)!;
            }
            catch
            {
                return false;
            }
            return @case != null;
        }

        /// <summary>
        /// Gets the <see cref="EventCaseDefinition"/> that applies to the specified event
        /// </summary>
        /// <param name="state">The <see cref="SwitchStateDefinition"/> to search for the <see cref="EventCaseDefinition"/> that applies to the specified event</param>
        /// <param name="eventReference">The name of the event the <see cref="EventCaseDefinition"/> to get applies to</param>
        /// <returns>The <see cref="EventCaseDefinition"/> that applies to the specified event</returns>
        public static EventCaseDefinition? GetEventCondition(this SwitchStateDefinition state, string eventReference)
        {
            return state.EventConditions?.FirstOrDefault(c => c.Event == eventReference);
        }

        /// <summary>
        /// Attempts to get the <see cref="EventCaseDefinition"/> that applies to the specified event
        /// </summary>
        /// <param name="state">The <see cref="SwitchStateDefinition"/> to search for the specified <see cref="EventCaseDefinition"/></param>
        /// <param name="eventReference">The reference of the event the <see cref="EventCaseDefinition"/> to get applies to</param>
        /// <param name="case">The <see cref="EventCaseDefinition"/> that applies to the specified event</param>
        /// <returns>A boolean indicating whether or not a <see cref="EventCaseDefinition"/> with the specified id could be found</returns>
        public static bool TryGetEventCondition(this SwitchStateDefinition state, string eventReference, out EventCaseDefinition @case)
        {
            @case = null!;
            try
            {
                @case = state.GetEventCondition(eventReference)!;
            }
            catch
            {
                return false;
            }
            return @case != null;
        }

    }

}
