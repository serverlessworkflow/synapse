using Newtonsoft.Json;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents the object used to describe a <see cref="V1Workflow"/>'s validation status
    /// </summary>
    public class V1WorkflowValidationStatus
    {

        /// <summary>
        /// Gets the <see cref="V1Workflow"/>'s validation state
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public V1WorkflowDefinitionStatus State { get; set; } = V1WorkflowDefinitionStatus.Pending;

    }

}
