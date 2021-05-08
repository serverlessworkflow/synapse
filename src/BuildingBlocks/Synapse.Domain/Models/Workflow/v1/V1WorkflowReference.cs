using Newtonsoft.Json;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents a reference to a <see cref="WorkflowDefinition"/>
    /// </summary>
    public class V1WorkflowReference
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowReference"/>
        /// </summary>
        public V1WorkflowReference()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowReference"/>
        /// </summary>
        /// <param name="id">The id of the <see cref="WorkflowDefinition"/> to reference</param>
        /// <param name="version">The version of the <see cref="WorkflowDefinition"/> to reference</param>
        public V1WorkflowReference(string id, string version)
            : this()
        {
            this.Id = id;
            this.Version = version;
        }

        /// <summary>
        /// Gets the id of the <see cref="WorkflowDefinition"/> to reference
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets the version of the <see cref="WorkflowDefinition"/> to reference
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

    }

}
