using Neuroglia.K8s;
using Synapse.Domain.Models;

namespace Synapse
{

    /// <summary>
    /// Exposes Synapse <see cref="ICustomResourceDefinition"/>s
    /// </summary>
    public static class SynapseCustomResources
    {

        /// <summary>
        /// Gets Synapse's <see cref="V1WorkflowDefinition"/>
        /// </summary>
        public static V1WorkflowDefinition Workflow = new();

        /// <summary>
        /// Gets Synapse's <see cref="V1WorkflowInstanceDefinition"/>
        /// </summary>
        public static V1WorkflowInstanceDefinition WorkflowInstance = new();

    }

}
