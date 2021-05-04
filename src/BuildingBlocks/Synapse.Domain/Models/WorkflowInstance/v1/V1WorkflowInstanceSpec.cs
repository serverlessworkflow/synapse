using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Synapse.Domain.Models
{

    public class V1WorkflowInstanceSpec
    {

        public V1WorkflowInstanceSpec()
        {

        }

        public V1WorkflowInstanceSpec(V1WorkflowReference definition, JObject input)
            : this()
        {
            this.Definition = definition;
            this.Input = input;
        }

        [JsonProperty("definition")]
        public V1WorkflowReference Definition { get; set; }

        [JsonProperty("input")]
        public JObject Input { get; set; }

    }

}
