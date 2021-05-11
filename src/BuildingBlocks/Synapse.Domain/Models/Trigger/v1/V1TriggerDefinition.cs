using Neuroglia.K8s;

namespace Synapse.Domain.Models
{
    /// <summary>
    /// Represents a <see cref="CustomResourceDefinition"/> used to represent an event trigger
    /// </summary>
    public class V1TriggerDefinition
        : CustomResourceDefinition
    {

        /// <summary>
        /// Gets the <see cref="V1TriggerDefinition"/>'s kind
        /// </summary>
        public const string KIND = "Trigger";
        /// <summary>
        /// Gets the <see cref="V1TriggerDefinition"/>'s plural
        /// </summary>
        public const string PLURAL = "triggers";

        /// <summary>
        /// Initializes a new <see cref="V1TriggerDefinition"/>
        /// </summary>
        public V1TriggerDefinition()
            : base(SynapseConstants.Resources.ApiVersion, KIND, PLURAL)
        {

        }

    }

}
