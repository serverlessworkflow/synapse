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

using CloudNative.CloudEvents;
using System.Runtime.Serialization;

namespace Synapse
{
    /// <summary>
    /// Enumerates all possible workflow activity types
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.StringEnumConverterFactory))]
    public enum V1WorkflowActivityType
    {
        /// <summary>
        /// Indicates an activity that processes the start of a workflow instance
        /// </summary>
        [EnumMember(Value = "start")]
        Start,
        /// <summary>
        /// Indicates an activity that processes a state definition's execution
        /// </summary>
        [EnumMember(Value = "state")]
        State,
        /// <summary>
        /// Indicates an activity that processes an action's execution
        /// </summary>
        [EnumMember(Value = "action")]
        Action,
        /// <summary>
        /// Indicates an activity that consumes an inbound <see cref="CloudEvent"/>
        /// </summary>
        [EnumMember(Value = "consume-event")]
        ConsumeEvent,
        /// <summary>
        /// Indicates an activity that produces an output <see cref="CloudEvent"/>
        /// </summary>
        [EnumMember(Value = "produce-event")]
        ProduceEvent,
        /// <summary>
        /// Indicates an activity that processes an event trigger
        /// </summary>
        [EnumMember(Value = "event-trigger")]
        EventTrigger,
        /// <summary>
        /// Indicates an activity that processes a function call
        /// </summary>
        [EnumMember(Value = "function")]
        Function,
        /// <summary>
        /// Indicates an activity that processes a workflow branch
        /// </summary>
        [EnumMember(Value = "branch")]
        Branch,
        /// <summary>
        /// Indicates an activity that processes a subflow
        /// </summary>
        [EnumMember(Value = "subflow")]
        SubFlow,
        /// <summary>
        /// Indicates an activity that processes a transition from a state to another
        /// </summary>
        [EnumMember(Value = "transition")]
        Transition,
        /// <summary>
        /// Indicates an activity that processes an iteration
        /// </summary>
        [EnumMember(Value = "iteration")]
        Iteration,
        /// <summary>
        /// Indicates an activity that processes the end of a workflow instance
        /// </summary>
        [EnumMember(Value = "end")]
        End,
        /// <summary>
        /// Indicates an activity that handles specific domain errors
        /// </summary>
        [EnumMember(Value = "error")]
        Error
    }

}
