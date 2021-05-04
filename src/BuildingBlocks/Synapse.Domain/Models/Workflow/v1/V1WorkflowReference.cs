using Newtonsoft.Json;

namespace Synapse.Domain.Models
{
    public class V1WorkflowReference
    {

        public V1WorkflowReference()
        {

        }

        public V1WorkflowReference(string id, string version)
            : this()
        {
            this.Id = id;
            this.Version = version;
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

    }

}
