using Neuroglia.K8s;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Domain.Models
{

    /// <summary>
    /// Represents a <see cref="CustomResourceDefinition"/> used to execute the instance of a <see cref="WorkflowDefinition"/>
    /// </summary>
    public class V1WorkflowInstanceDefinition
        : CustomResourceDefinition
    {

        /// <summary>
        /// Gets the <see cref="V1WorkflowInstanceDefinition"/>'s kind
        /// </summary>
        public const string KIND = "WorkflowInstance";
        /// <summary>
        /// Gets the <see cref="V1WorkflowInstanceDefinition"/>'s plural
        /// </summary>
        public const string PLURAL = "workflow-instances";

        /// <summary>
        /// Initializes a new <see cref="V1WorkflowInstanceDefinition"/>
        /// </summary>
        public V1WorkflowInstanceDefinition()
            : base(SynapseConstants.Resources.ApiVersion, KIND, PLURAL)
        {

        }

    }

}
