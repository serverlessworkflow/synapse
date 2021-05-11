using Newtonsoft.Json;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents a <see cref="V1Workflow"/>'s spec
    /// </summary>
    public class V1WorkflowSpec
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowSpec"/>
        /// </summary>
        public V1WorkflowSpec()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowSpec"/>
        /// </summary>
        /// <param name="definition">The <see cref="V1Workflow"/>'s <see cref="WorkflowDefinition"/></param>
        public V1WorkflowSpec(WorkflowDefinition definition)
            : this()
        {
            this.Definition = definition;
        }

        /// <summary>
        /// Gets the <see cref="V1Workflow"/>'s <see cref="WorkflowDefinition"/>
        /// </summary>
        [JsonProperty("definition")]
        public WorkflowDefinition Definition { get; set; }

    }

}
