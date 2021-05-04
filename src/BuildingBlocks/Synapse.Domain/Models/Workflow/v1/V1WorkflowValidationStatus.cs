using Newtonsoft.Json;

namespace Synapse.Domain.Models
{
    public class V1WorkflowValidationStatus
    {

        [JsonProperty(PropertyName = "state")]
        public V1WorkflowDefinitionStatus State { get; set; } = V1WorkflowDefinitionStatus.Pending;

    }

}
