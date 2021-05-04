using k8s.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents the status of a <see cref="V1WorkflowInstance"/>
    /// </summary>
    public class V1WorkflowInstanceStatus
    {

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s status type
        /// </summary>
        [JsonProperty("type")]
        public V1WorkflowActivityStatus Type { get; set; }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1WorkflowInstance"/> has been initialized
        /// </summary>
        [JsonProperty("initializedAt")]
        public DateTimeOffset? InitializedAt { get; set; }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1WorkflowInstance"/> has been deployed
        /// </summary>
        [JsonProperty("deployedAt")]
        public DateTimeOffset? DeployedAt { get; set; }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1WorkflowInstance"/> has started
        /// </summary>
        [JsonProperty("startedAt")]
        public DateTimeOffset? StartedAt { get; set; }

        /// <summary>
        /// Gets the date and time at which the <see cref="V1WorkflowInstance"/> has been executed
        /// </summary>
        [JsonProperty("executedAt")]
        public DateTimeOffset? ExecutedAt { get; set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s pod <see cref="V1ObjectReference"/>
        /// </summary>
        [JsonProperty("pod")]
        public V1ObjectReference Pod { get; set; }

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the <see cref="V1ExecutionInterruption"/>s that have occured during the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        [JsonProperty("interruptions")]
        public List<V1ExecutionInterruption> Interruptions { get; set; } = new List<V1ExecutionInterruption>();

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the <see cref="V1WorkflowActivity"/> instances that have occured during the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        [JsonProperty("activityLog")]
        public List<V1WorkflowActivity> ActivityLog { get; set; } = new List<V1WorkflowActivity>();

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the <see cref="V1Error"/>s that have occured during the <see cref="V1WorkflowInstance"/>'s execution
        /// </summary>
        [JsonProperty("errors")]
        public List<V1Error> Errors { get; set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s output
        /// </summary>
        [JsonProperty("output")]
        public JToken Output { get; set; }

    }

}
