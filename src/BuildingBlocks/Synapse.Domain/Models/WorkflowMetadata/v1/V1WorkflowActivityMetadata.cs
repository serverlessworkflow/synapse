using Newtonsoft.Json;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents the metadata of a <see cref="V1WorkflowActivity"/>
    /// </summary>
    public class V1WorkflowActivityMetadata
    {

        /// <summary>
        /// Gets/sets the name of the <see cref="StateDefinition"/> the <see cref="V1WorkflowActivity"/> processes/belongs to
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }

        /// <summary>
        /// Gets/sets the name of the <see cref="ActionDefinition"/> the <see cref="V1WorkflowActivity"/> processes
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        /// Gets/sets the name of the <see cref="SwitchCaseDefinition"/> the <see cref="V1WorkflowActivity"/> processes
        /// </summary>
        [JsonProperty("case")]
        public string Case { get; set; }

        /// <summary>
        /// Gets/sets the id of the <see cref="EventStateTriggerDefinition"/> the <see cref="V1WorkflowActivity"/> processes
        /// </summary>
        [JsonProperty("triggerId")]
        public int? TriggerId { get; set; }

        /// <summary>
        /// Gets/sets the name of the <see cref="EventDefinition"/> the <see cref="V1WorkflowActivity"/> processes
        /// </summary>
        [JsonProperty("event")]
        public string Event { get; set; }

    }

}
