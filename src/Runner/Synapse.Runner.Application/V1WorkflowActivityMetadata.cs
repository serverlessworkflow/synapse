using ServerlessWorkflow.Sdk.Models;
using Synapse.Domain.Models;

namespace Synapse.Runner.Application
{

    /// <summary>
    /// Exposeses constants about <see cref="V1WorkflowActivity"/> metadata
    /// </summary>
    public static class V1WorkflowActivityMetadata
    {

        /// <summary>
        /// Gets the key of the metadata used to store the name of the <see cref="ActionDefinition"/> the <see cref="V1WorkflowActivity"/> processes
        /// </summary>
        public const string Action = nameof(Action);

        /// <summary>
        /// Gets the key of the metadata used to store the name of the <see cref="BranchDefinition"/> the <see cref="V1WorkflowActivity"/> processes
        /// </summary>
        public const string Branch = nameof(Branch);

        /// <summary>
        /// Gets the key of the metadata used to store the name of the <see cref="StateDefinition"/> the <see cref="V1WorkflowActivity"/> processes
        /// </summary>
        public const string State = nameof(State);


    }
}
