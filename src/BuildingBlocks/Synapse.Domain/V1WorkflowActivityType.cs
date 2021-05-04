using CloudNative.CloudEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Synapse.Domain.Models;
using System.Runtime.Serialization;

namespace Synapse
{

    /// <summary>
    /// Enumerates all types of <see cref="V1WorkflowActivity"/>
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum V1WorkflowActivityType
    {
        /// <summary>
        /// Indicates an activity that processes the start of a workflow instance
        /// </summary>
        [EnumMember(Value = "START")]
        Start,
        /// <summary>
        /// Indicates an activity that processes a state definition's execution
        /// </summary>
        [EnumMember(Value = "STATE")]
        State,
        /// <summary>
        /// Indicates an activity that processes an action's execution
        /// </summary>
        [EnumMember(Value = "ACTION")]
        Action,
        /// <summary>
        /// Indicates an activity that consumes an inbound <see cref="CloudEvent"/>
        /// </summary>
        [EnumMember(Value = "EVENT_CONSUME")]
        ConsumeEvent,
        /// <summary>
        /// Indicates an activity that produces an output <see cref="CloudEvent"/>
        /// </summary>
        [EnumMember(Value = "EVENT_PRODUCE")]
        ProduceEvent,
        /// <summary>
        /// Indicates an activity that processes an event trigger
        /// </summary>
        [EnumMember(Value = "EVENTTRIGGER")]
        EventTrigger,
        /// <summary>
        /// Indicates an activity that processes a function call
        /// </summary>
        [EnumMember(Value = "FUNCTION")]
        Function,
        /// <summary>
        /// Indicates an activity that processes a workflow branch
        /// </summary>
        [EnumMember(Value = "BRANCH")]
        Branch,
        /// <summary>
        /// Indicates an activity that processes a subflow
        /// </summary>
        [EnumMember(Value = "SUBFLOW")]
        SubFlow,
        /// <summary>
        /// Indicates an activity that processes a transition from a state to another
        /// </summary>
        [EnumMember(Value = "TRANSITION")]
        Transition,
        /// <summary>
        /// Indicates an activity that processes an iteration
        /// </summary>
        [EnumMember(Value = "ITERATION")]
        Iteration,
        /// <summary>
        /// Indicates an activity that processes the end of a workflow instance
        /// </summary>
        [EnumMember(Value = "END")]
        End,
        /// <summary>
        /// Indicates an activity that handles specific domain errors
        /// </summary>
        [EnumMember(Value = "ERROR")]
        Error
    }

}
