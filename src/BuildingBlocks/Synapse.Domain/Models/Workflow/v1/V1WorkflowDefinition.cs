using Neuroglia.K8s;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents a <see cref="CustomResourceDefinition"/> used to deploy a <see cref="WorkflowDefinition"/>
    /// </summary>
    public class V1WorkflowDefinition
        : CustomResourceDefinition
    {

        /// <summary>
        /// Gets the <see cref="V1WorkflowDefinition"/>'s kind
        /// </summary>
        public const string KIND = "Workflow";
        /// <summary>
        /// Gets the <see cref="V1WorkflowDefinition"/>'s plural
        /// </summary>
        public const string PLURAL = "workflows";

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowDefinition"/>
        /// </summary>
        public V1WorkflowDefinition() 
            : base(SynapseConstants.Resources.ApiVersion, KIND, PLURAL)
        {

        }

    }

}
