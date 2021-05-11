using Newtonsoft.Json;
using System;

namespace Synapse.Domain.Models
{
    /// <summary>
    /// Represents the object used to configure the outcome of a <see cref="V1Trigger"/>
    /// </summary>
    public class V1TriggerOutcome
    {

        /// <summary>
        /// Initializes a new <see cref="V1TriggerOutcome"/>
        /// </summary>
        public V1TriggerOutcome()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1TriggerOutcome"/>
        /// </summary>
        /// <param name="type">The <see cref="V1TriggerOutcome"/>'s type</param>
        /// <param name="workflow">A reference to the <see cref="V1Workflow"/> to run or resume</param>
        public V1TriggerOutcome(V1TriggerOutcomeType type, V1WorkflowReference workflow)
            : this()
        {
            this.Type = type;
            this.Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
        }

        /// <summary>
        /// Initializes a new <see cref="V1TriggerOutcome"/>
        /// </summary>
        /// <param name="type">The <see cref="V1TriggerOutcome"/>'s type</param>
        /// <param name="workflow">A reference to the <see cref="V1Workflow"/> to run or resume</param>
        /// <param name="workflowInstance">The name of the <see cref="V1WorkflowInstance"/> to run or resume</param>
        public V1TriggerOutcome(V1TriggerOutcomeType type, V1WorkflowReference workflow, string workflowInstance)
            : this(type, workflow)
        {
            this.WorkflowInstance = workflowInstance;
        }

        /// <summary>
        /// Gets the <see cref="V1TriggerOutcome"/>'s type
        /// </summary>
        [JsonProperty("type")]
        public V1TriggerOutcomeType Type { get; set; }

        /// <summary>
        /// Gets a reference to the <see cref="V1Workflow"/> to run or resume
        /// </summary>
        [JsonProperty("workflow")]
        public V1WorkflowReference Workflow { get; set; }

        /// <summary>
        /// Gets the name of the <see cref="V1WorkflowInstance"/> to run or resume
        /// </summary>
        [JsonProperty("workflowInstance")]
        public string WorkflowInstance { get; set; }

    }

}
