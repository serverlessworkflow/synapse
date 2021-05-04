using Newtonsoft.Json;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Domain.Models
{
    public class V1WorkflowSpec
    {

        public V1WorkflowSpec()
        {

        }

        public V1WorkflowSpec(WorkflowDefinition definition)
            : this()
        {
            this.Definition = definition;
        }

        [JsonProperty("definition")]
        public WorkflowDefinition Definition { get; set; }

    }

}
