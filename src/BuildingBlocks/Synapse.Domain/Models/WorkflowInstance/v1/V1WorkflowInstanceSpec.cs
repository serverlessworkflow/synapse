using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents a <see cref="V1WorkflowInstance"/> spec
    /// </summary>
    public class V1WorkflowInstanceSpec
    {

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceSpec"/>
        /// </summary>
        public V1WorkflowInstanceSpec()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceSpec"/>
        /// </summary>
        /// <param name="definition">A <see cref="V1WorkflowReference"/> to the <see cref="V1WorkflowInstance"/>'s <see cref="V1Workflow"/></param>
        /// <param name="input">The <see cref="V1WorkflowInstance"/>'s input data</param>
        public V1WorkflowInstanceSpec(V1WorkflowReference definition, JObject input)
            : this()
        {
            this.Definition = definition ?? throw DomainException.ArgumentNull(nameof(definition));
            this.Input = input ?? throw DomainException.ArgumentNull(nameof(input));
        }

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceSpec"/>
        /// </summary>
        /// <param name="definition">A <see cref="V1WorkflowReference"/> to the <see cref="V1WorkflowInstance"/>'s <see cref="V1Workflow"/></param>
        /// <param name="input">The <see cref="V1WorkflowInstance"/>'s input data</param>
        /// <param name="correlationContext">The <see cref="V1WorkflowInstance"/>'s <see cref="V1CorrelationContext"/></param>
        public V1WorkflowInstanceSpec(V1WorkflowReference definition, JObject input, V1CorrelationContext correlationContext)
            : this(definition, input)
        {
            this.CorrelationContext = correlationContext ?? throw DomainException.ArgumentNull(nameof(correlationContext));
        }

        /// <summary>
        /// Gets a <see cref="V1WorkflowReference"/> to the <see cref="V1WorkflowInstance"/>'s <see cref="V1Workflow"/>
        /// </summary>
        [JsonProperty("definition")]
        public V1WorkflowReference Definition { get; set; }

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s input data
        /// </summary>
        [JsonProperty("input")]
        public JObject Input { get; set; } = new JObject();

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstance"/>'s <see cref="V1CorrelationContext"/>
        /// </summary>
        [JsonProperty("correlationContext")]
        public V1CorrelationContext CorrelationContext { get; set; } = new();

    }

}
